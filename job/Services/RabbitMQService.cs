using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace job.Services
{
    public interface IRabbitMQService
    {
        void PublishApplicantEvaluationTask(int applicationId, string userId, int jobId, string cvUrl);
    }

    public class RabbitMQService : IRabbitMQService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void PublishApplicantEvaluationTask(int applicationId, string userId, int jobId, string cvUrl)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_configuration.GetConnectionString("RabbitMQ")!)
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: "job_application_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var payload = new
                {
                    task_type = "evaluate_applicant",
                    payload = new
                    {
                        application_id = applicationId,
                        user_id = userId,
                        job_id = jobId,
                        cv_url = cvUrl
                    }
                };

                var message = JsonSerializer.Serialize(payload);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: "job_application_queue",
                                     basicProperties: properties,
                                     body: body);

                _logger.LogInformation($"[RabbitMQ] Published evaluate_applicant task for Application {applicationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RabbitMQ] Could not publish message to RabbitMQ.");
            }
        }
    }
}

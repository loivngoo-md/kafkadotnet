using System.Text.Json;
using kafkadotnet.dto;
using kafkadotnet.Infrastructures.kafka;
using Microsoft.AspNetCore.Mvc;

namespace kafkadotnet.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{

    private readonly string topic = "EmailTopic";

    [HttpPost(Name = "SendEmail")]
    public async Task<IActionResult> Post([FromBody] SendEmailDTO request)
    {
        try
        {
            string message = JsonSerializer.Serialize(request);
            return Ok(await KafkaProducer.SendToKafka(topic, message));
        }
        catch (Exception e)
        {

            throw new Exception(e.Message);
        }
    }

    [HttpGet(Name = "SendEmail")]
    public async Task<IActionResult> Get()
    {
        return Ok(await KafkaProducer.SendToKafka(topic, "TEST Thanh cong"));
    }

}
using BankService.Models;

namespace BankService.Clients
{
    public interface IRabbitMqClient
    {
        void SendMessage(HistoryMessage historyMessage);
    }
}
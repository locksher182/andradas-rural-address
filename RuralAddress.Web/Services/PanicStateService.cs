using System;

namespace RuralAddress.Web.Services
{
    // Esta classe funciona como uma "Sala de Controle" (Singleton)
    public class PanicStateService
    {
        // --- CANAL 1: PÂNICO (Já existia) ---
        // Parâmetros: ID, Nome, Lat, Lon
        public event Action<int, string, double, double>? OnPanicTriggered;

        public void TriggerPanic(int id, string nome, double lat, double lon)
        {
            // Avisa o site sobre o pânico
            OnPanicTriggered?.Invoke(id, nome, lat, lon);
        }

        // --- CANAL 2: CHAT (Novo) ---
        // Parâmetros: ID Pessoa, Nome Pessoa, Mensagem
        public event Action<int, string, string>? OnChatReceived;

        // O Controller vai chamar este método quando chegar mensagem do celular
        public void TriggerChat(int id, string nome, string mensagem)
        {
            // Avisa o site que chegou mensagem nova
            OnChatReceived?.Invoke(id, nome, mensagem);
        }
    }
}
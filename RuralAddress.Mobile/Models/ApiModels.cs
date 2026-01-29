using System;

namespace RuralAddress.Mobile.Models
{
    public class MobileLoginRequest
    {
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public class MobileLoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public int PessoaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class PanicAlertRequest
    {
        public int PessoaId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class MobileChatRequest
    {
        public int PessoaId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

using System;

namespace RuralAddress.Web.Models.Api
{
    public class MobileLoginRequest
    {
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public class MobileLoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty; // For now maybe just the PersonId
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
}

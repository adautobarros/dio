namespace FunctionsValidaCpf
{
    public class Cpf
    {
        public string Documento { get; set; }

        public bool Valido()
        {

            // Remove caracteres não numéricos
            Documento = Documento.Replace(".", "").Replace("-", "");

            // Verifica se o CPF possui 11 dígitos
            if (Documento.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais
            if (Enumerable.Range(0, 11).All(i => Documento[i] == Documento[0]))
                return false;

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = Documento.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return Documento.EndsWith(digito);
        }
    }
}

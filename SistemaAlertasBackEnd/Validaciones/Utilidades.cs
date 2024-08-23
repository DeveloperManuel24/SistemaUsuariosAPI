namespace SistemaAlertasBackEnd.Validaciones
{
    public static class Utilidades
    {
        public static string CampoRequeridoMensaje = "El campo {PropertyName} es requerido";
        public static string MaximumLengthMensaje = "El campo {PropertyName} debe tener menos de {MaxLength} caracteres";
        public static string PrimeraLetraMayuscula = "El campo {PropertyName} debe comenzar con mayúsculas";
        public static string EmailMensaje = "El campo {PropertyName} debe ser un email válido";

        public static string GreaterThanOrEqualToMensaje(DateTime fechaMinima)
        {
            return $"El campo {{PropertyName}} debe ser posterior a {fechaMinima.ToString("yyyy-MM-dd")}";
        }

        //Regla personalizada:

        public static bool PrimeraLetraEnMayusculas(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return true;
            }

            var primeraLetra = valor[0].ToString();

            return primeraLetra == primeraLetra.ToUpper();
        }
    }
}

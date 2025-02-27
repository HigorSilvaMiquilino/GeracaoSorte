using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GeracaoSorte.Data
{    public class ClienteComNumeros
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public int QtdNumSorteRegular { get; set; }
        public int NumerosGerados { get; set; }
        public string Serie { get; set; }
        public string Ordem { get; set; }
        public string NumerosDaSorte { get; set; }
    }
}

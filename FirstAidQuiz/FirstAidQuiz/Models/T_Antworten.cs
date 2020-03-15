namespace FirstAidQuiz {

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class T_Antworten {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int P_Id { get; set; }

        [Required]
        public string Antwort { get; set; }

        public bool Richtig { get; set; }
    }
}
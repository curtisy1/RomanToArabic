namespace FirstAidQuiz {

    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Antworten")]
    public partial class Antworten {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Antworten() {
            this.Fragens = new HashSet<Fragen>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int P_Id { get; set; }

        [Required]
        public string Antwort { get; set; }

        public bool Richtig { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Fragen> Fragens { get; set; }
    }
}
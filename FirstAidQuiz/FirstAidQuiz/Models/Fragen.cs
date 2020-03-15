namespace FirstAidQuiz {

    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Fragen")]
    public partial class Fragen {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Fragen() {
            this.Antwortens = new HashSet<Antworten>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int P_Id { get; set; }

        public string Frage { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Antworten> Antwortens { get; set; }
    }
}
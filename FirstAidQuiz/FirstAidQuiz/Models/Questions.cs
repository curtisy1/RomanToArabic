namespace FirstAidQuiz {

    using System;
    using System.Data.Entity;

    public partial class Questions : DbContext {

        public Questions()
            : base("name=Questions") {
            Database.SetInitializer<Questions>(null);
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Directory.GetCurrentDirectory()); // meh..
        }

        public virtual DbSet<Antworten> Antwortens { get; set; }
        public virtual DbSet<Fragen> Fragens { get; set; }
        public virtual DbSet<T_Antworten> T_Antworten { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<Antworten>()
                .HasMany(e => e.Fragens)
                .WithMany(e => e.Antwortens)
                .Map(m => m.ToTable("FrageAntwort").MapLeftKey("P_FK_IdAntwort").MapRightKey("P_FK_IdFragen"));

            modelBuilder.Entity<Fragen>()
                .Property(e => e.Frage)
                .IsUnicode(false);
        }
    }
}
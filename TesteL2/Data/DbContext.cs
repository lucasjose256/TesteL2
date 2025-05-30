using TesteL2.Models;

namespace TesteL2.Data;

using Microsoft.EntityFrameworkCore;


    public class LojaContext : DbContext
    {
        public LojaContext(DbContextOptions<LojaContext> options) : base(options)
        {
        }

        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Caixa> Caixas { get; set; }
        public DbSet<ProcessamentoLog> ProcessamentosLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Produto>()
                .HasOne(p => p.Pedido)
                .WithMany(pe => pe.Produtos)
                .HasForeignKey(p => p.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProcessamentoLog>()
                .HasOne(pl => pl.Pedido)
                .WithMany(p => p.ProcessamentosLog)
                .HasForeignKey(pl => pl.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Caixa>().HasData(
                new Caixa { Id = 1, Nome = "Caixa 1", Altura = 30, Largura = 40, Comprimento = 80 },
                new Caixa { Id = 2, Nome = "Caixa 2", Altura = 80, Largura = 50, Comprimento = 40 },
                new Caixa { Id = 3, Nome = "Caixa 3", Altura = 50, Largura = 80, Comprimento = 60 }
            );
            
            modelBuilder.Entity<Produto>()
                .HasIndex(p => p.PedidoId);

            modelBuilder.Entity<ProcessamentoLog>()
                .HasIndex(pl => pl.PedidoId);

            modelBuilder.Entity<ProcessamentoLog>()
                .HasIndex(pl => pl.DataProcessamento);
        }
    }

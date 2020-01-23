using Orleans.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.TestingHost;
using Orleans.Transactions.TestKit;
using Orleans.Transactions.Tests;
using TestExtensions;
using Tester;

namespace Orleans.Transactions.AzureStorage.Tests {
  public class TestFixture : BaseTestClusterFixture {
    protected override void CheckPreconditionsOrThrow() {
      base.CheckPreconditionsOrThrow();
      TestUtils.CheckForAzureStorage();
    }

    protected override void ConfigureTestCluster(TestClusterBuilder builder) {
      builder.AddSiloBuilderConfigurator<SiloBuilderConfigurator>();
    }

    public class SiloBuilderConfigurator : ISiloConfigurator {
      public void Configure(ISiloBuilder hostBuilder) {
        hostBuilder
            .ConfigureServices(
                services =>
                    services.AddSingletonNamedService<IRemoteCommitService,
                                                      RemoteCommitService>(
                        TransactionTestConstants.RemoteCommitService))
            .ConfigureTracingForTransactionTests()
            .AddAzureTableTransactionalStateStorage(
                TransactionTestConstants.TransactionStore,
                options => {
                  options.ConnectionString =
                      TestDefaultConfiguration.DataConnectionString;
                })
            .UseTransactions();
      }
    }
  }

  public class ControlledFaultInjectionTestFixture : BaseTestClusterFixture {
    protected override void CheckPreconditionsOrThrow() {
      base.CheckPreconditionsOrThrow();
      TestUtils.CheckForAzureStorage();
    }

    protected override void ConfigureTestCluster(TestClusterBuilder builder) {
      builder.AddSiloBuilderConfigurator<SiloBuilderConfigurator>();
    }

    public class SiloBuilderConfigurator : ISiloConfigurator {
      public void Configure(ISiloBuilder hostBuilder) {
        hostBuilder.ConfigureTracingForTransactionTests()
            .AddFaultInjectionAzureTableTransactionalStateStorage(
                TransactionTestConstants.TransactionStore,
                options => {
                  options.ConnectionString =
                      TestDefaultConfiguration.DataConnectionString;
                })
            .UseControlledFaultInjectionTransactionState()
            .UseTransactions()
            .ConfigureServices(svc => {
              svc.AddScoped<ITransactionFaultInjector,
                            SimpleAzureStorageExceptionInjector>()
                  .AddScoped<IControlledTransactionFaultInjector>(
                      sp => sp.GetService<ITransactionFaultInjector>()
                                as IControlledTransactionFaultInjector);
            });
      }
    }
  }

  public class SkewedClockTestFixture : TestFixture {
    protected override void ConfigureTestCluster(TestClusterBuilder builder) {
      builder.AddSiloBuilderConfigurator<SkewedClockConfigurator>();
      base.ConfigureTestCluster(builder);
    }
  }

  public class RandomFaultInjectedTestFixture : TestFixture {
    protected override void ConfigureTestCluster(TestClusterBuilder builder) {
      builder.AddSiloBuilderConfigurator<TxSiloBuilderConfigurator>();
      base.ConfigureTestCluster(builder);
    }

    public class TxSiloBuilderConfigurator : ISiloConfigurator {
      private static readonly double probability = 0.05;
      public void Configure(ISiloBuilder hostBuilder) {
        hostBuilder.ConfigureTracingForTransactionTests()
            .AddFaultInjectionAzureTableTransactionalStateStorage(
                TransactionTestConstants.TransactionStore,
                options => {
                  options.ConnectionString =
                      TestDefaultConfiguration.DataConnectionString;
                })
            .UseTransactions()
            .ConfigureServices(
                services => services.AddSingleton<ITransactionFaultInjector>(
                    sp => new RandomErrorInjector(probability)));
      }
    }
  }

}

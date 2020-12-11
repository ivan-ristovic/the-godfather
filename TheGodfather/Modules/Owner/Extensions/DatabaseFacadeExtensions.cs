using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using TheGodfather.Database;

namespace TheGodfather.Modules.Owner.Extensions
{
    public static class DatabaseFacadeExtensions
    {
        public static async Task<RelationalDataReader> ExecuteSqlQueryAsync(this DatabaseFacade databaseFacade,
                                                                            string sql,
                                                                            TheGodfatherDbContext context,
                                                                            params object[] parameters)
        {

            IConcurrencyDetector concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();

            using (concurrencyDetector.EnterCriticalSection()) {
                RawSqlCommand rawSqlCommand = databaseFacade
                    .GetService<IRawSqlCommandBuilder>()
                    .Build(sql, parameters);

                return await rawSqlCommand
                    .RelationalCommand
                    .ExecuteReaderAsync(
                        new RelationalCommandParameterObject(
                            databaseFacade.GetService<IRelationalConnection>(),
                            parameterValues: rawSqlCommand.ParameterValues,
                            null,
                            context,
                            null
                        )
                    );
            }
        }
    }
}

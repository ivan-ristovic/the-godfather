#region USING_DIRECTIVES
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Database;
#endregion

namespace TheGodfather.Modules.Administration.Extensions
{
    public static class DatabaseFacadeExtensions
    {
        public static async Task<RelationalDataReader> ExecuteSqlQueryAsync(this DatabaseFacade databaseFacade,
                                                                            string sql,
                                                                            DatabaseContext context,
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
                            context,
                            null
                        )
                    );
            }
        }
    }
}

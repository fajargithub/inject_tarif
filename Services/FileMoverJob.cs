using ExcelDataReader;
using InjectServiceWorker.Models;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Oracle.ManagedDataAccess.Client;
using Quartz;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectServiceWorker.Services
{
    public class FileMoverJob : IJob
    {
        private readonly ApplicationDbContext _dbContext;
        public FileMoverJob(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string sourcePath = OperatingSystem.IsWindows() ? NetworkPaths.sourcePath : "/mnt/source_path";
            string donePath = OperatingSystem.IsWindows() ? NetworkPaths.donePath : "/mnt/done_path";
            string invalidPath = OperatingSystem.IsWindows() ? NetworkPaths.invalidPath : "/mnt/invalid_path";
            string errorPath = OperatingSystem.IsWindows() ? NetworkPaths.errorPath : "/mnt/error_path";

            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine("Source directory does not exist");
                return;
            }

            try
            {
                // Create an ArrayList to store all rows from all files
                List<ServiceRateProviderModel> allRows = new List<ServiceRateProviderModel>();

                // Get all Excel files in the folder
                var excelFiles = Directory.GetFiles(sourcePath)
                    .Where(f => f.EndsWith(".xls") || f.EndsWith(".xlsx"));

                foreach (var filePath in excelFiles)
                {
                    IWorkbook workbook;

                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        if (Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                        {
                            workbook = new XSSFWorkbook(stream);
                        }
                        else
                        {
                            workbook = new HSSFWorkbook(stream);
                        }

                        Console.WriteLine($"Reading file: {Path.GetFileName(filePath)}");

                        // Get the first sheet
                        var sheet = workbook.GetSheetAt(0);

                        // Read all rows
                        for (int row = 0; row <= sheet.LastRowNum; row++)
                        {
                            var currentRow = sheet.GetRow(row);
                            if (currentRow != null)
                            {
                                // Create a list for the current row's data
                                ArrayList rowData = new ArrayList();

                                for (int col = 0; col < currentRow.LastCellNum; col++)
                                {
                                    rowData.Add(currentRow.GetCell(col)?.ToString());
                                }

                                // Add the row to our main collection
                                //allRows.Add(rowData);
                                if (rowData.Count > 0)
                                { 
                                    ServiceRateProviderModel model = new ServiceRateProviderModel();
                                    model.kd_tarif = rowData[0] != null ? rowData[0].ToString() : "";
                                    model.nm_tarif = rowData[1] != null ? rowData[1].ToString() : "";
                                    model.hg_jua = rowData[2] != null ? Decimal.Parse(rowData[2].ToString()) : 0;
                                    model.provider_code = rowData[3] != null ? rowData[3].ToString() : "";
                                    model.kd_holding = rowData[4] != null ? rowData[4].ToString() : "";
                                    model.item_id = rowData[5] != null ? rowData[5].ToString() : "";
                                    model.disc = rowData[10] != null ? Decimal.Parse(rowData[10].ToString()) : 0;
                                    model.disc_rp = rowData[11] != null ? Decimal.Parse(rowData[11].ToString()) : 0;
                                    model.kd_tarif_payor = rowData[12] != null ? rowData[12].ToString() : "";
                                    model.nm_tarif_payor = rowData[13] != null ? rowData[13].ToString() : "";
                                    model.kd_tarif_pro = rowData[14] != null ? rowData[14].ToString() : "";
                                    model.nm_tarif_pro = rowData[15] != null ? rowData[15].ToString() : "";
                                    model.efective_date = rowData[16] != null ? DateTime.ParseExact(rowData[16].ToString(), "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture) : null;
                                    model.hg_jua_new = rowData[17] != null ? Decimal.Parse(rowData[17].ToString()) : 0;
                                    model.agreement_type = rowData[18] != null ? rowData[18].ToString() : "";

                                    await InsertTblRateProvider(model);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        public async Task InsertTblRateProvider(ServiceRateProviderModel param)
        {
            try
            {
                // Execute the stored procedure
                await _dbContext.Database.ExecuteSqlRawAsync(
                         "BEGIN INSERT_LOG_SCANDOC(:p_kd_tarif, :p_nm_tarif, :p_hg_jua, :p_provider_code, " +
                         ":p_kd_holding, :p_item_id, :p_disc, :p_disc_rp, :p_kd_tarif_payor, " +
                         ":p_nm_tarif_payor, :p_kd_tarif_pro, :p_nm_tarif_pro, :p_efective_date, :p_hg_jua_new, :p_agreement_type, :cv_1); END;",
                         new OracleParameter("p_kd_tarif", param.kd_tarif),
                         new OracleParameter("p_nm_tarif", param.nm_tarif),
                         new OracleParameter("p_hg_jua", param.hg_jua),
                         new OracleParameter("p_provider_code", param.provider_code),
                         new OracleParameter("p_kd_holding", param.kd_holding),
                         new OracleParameter("p_item_id", param.item_id),
                         new OracleParameter("p_disc", param.disc),
                         new OracleParameter("p_disc_rp", param.disc_rp),
                         new OracleParameter("p_kd_tarif_payor", param.kd_tarif_payor),
                         new OracleParameter("p_nm_tarif_payor", param.nm_tarif_payor),
                         new OracleParameter("p_kd_tarif_pro", param.kd_tarif_pro),
                         new OracleParameter("p_nm_tarif_pro", param.nm_tarif_pro),
                         new OracleParameter("p_efective_date", OracleDbType.Date, param.efective_date, System.Data.ParameterDirection.Input),
                         new OracleParameter("p_hg_jua_new", param.hg_jua_new),
                         new OracleParameter("p_agreement_type", param.agreement_type),
                         new OracleParameter("cv_1", OracleDbType.RefCursor, System.Data.ParameterDirection.Output)
                     );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

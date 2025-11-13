using InjectServiceWorker.Models;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Oracle.ManagedDataAccess.Client;
using Quartz;
using System.Collections;

namespace InjectServiceWorker.Services
{
    public class InjectServiceOCR : IJob
    {
        private readonly ApplicationDbContext _dbContext;
        public InjectServiceOCR(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string sourcePath = OperatingSystem.IsWindows() ? NetworkPaths.sourcePath2 : "/mnt/source_path";
            string donePath = OperatingSystem.IsWindows() ? NetworkPaths.donePath2 : "/mnt/done_path";
            string invalidPath = OperatingSystem.IsWindows() ? NetworkPaths.invalidPath2 : "/mnt/invalid_path";
            string errorPath = OperatingSystem.IsWindows() ? NetworkPaths.errorPath2 : "/mnt/error_path";

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

                        var filename = Path.GetFileName(filePath);
                        //var arrFilename = filename.Split("_");
                        //if(arrFilename.Length != 3)
                        //{
                        //    //Move file here
                        //    MoveFile(filePath, invalidPath);
                        //    continue;
                        //}

                        //var kd_holding = arrFilename[0].Trim();
                        //var provider_code = arrFilename[1].Trim();

                        // Get the first sheet
                        var sheet = workbook.GetSheetAt(0);

                        // Read all rows
                        for (int row = 0; row <= sheet.LastRowNum; row++)
                        {
                            var currentRow = sheet.GetRow(row);
                            if (currentRow != null)
                            {
                                ArrayList rowData = new ArrayList();

                                for (int col = 0; col < currentRow.LastCellNum; col++)
                                {
                                    rowData.Add(currentRow.GetCell(col)?.ToString());
                                }

                                if (rowData.Count > 0 && row > 0)
                                {
                                    InjectServiceOCRModel model = new InjectServiceOCRModel();
                                    if (rowData[0] != null)
                                    {
                                        model.upload_reff = filename;
                                        model.provider_service_code = rowData[11].ToString();
                                        model.payor_code_indemnity = rowData[0].ToString();
                                        model.payor_code_mc = rowData[1].ToString();
                                        model.agreement_type_id = rowData[2].ToString();
                                        model.provider_code = rowData[10].ToString();
                                        model.service_name = rowData[12].ToString();
                                        model.service_class = rowData[14].ToString();
                                        model.price = rowData[15] != null ? decimal.Parse(rowData[15].ToString()) : 0;
                                        model.fixed_price = rowData[16] != null ? decimal.Parse(rowData[16].ToString()) : 0;
                                        model.disc = rowData[17] != null ? decimal.Parse(rowData[17].ToString()) : 0;
                                        model.disc_amount = 0;
                                        model.effective_date = rowData[31] != null ? DateTime.ParseExact(rowData[31].ToString(), "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture) : null;
                                        model.admedika_detail_service_code = rowData[7].ToString();
                                        await InsertTblRateIndemnity(model);
                                    }
                                }
                            }
                        }

                        //Move file here
                        //MoveFile(filePath, donePath);
                    }
                }

                //await UpdateInjectTblRateProvider();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task InsertTblRateIndemnity(InjectServiceOCRModel param)
        {
            try
            {
                // Execute the stored procedure
                var result = await _dbContext.Database.ExecuteSqlRawAsync(
                         "BEGIN INSERT_TBL_SERVICE_PROVIDER(:p_provider_service_code, :p_upload_reff, :p_payor_code_indemnity, :p_payor_code_mc, :p_agreement_type_id, :p_provider_code, :p_service_name, " +
                         ":p_service_class, :p_price, :p_fixed_price, :p_disc, :p_disc_amount, :p_effective_date, :p_admedika_detail_service_code, :cv_1); END;",
                         new OracleParameter("p_provider_service_code", param.provider_service_code),
                         new OracleParameter("p_upload_reff", param.upload_reff),
                         new OracleParameter("p_payor_code_indemnity", param.payor_code_indemnity),
                         new OracleParameter("p_payor_code_mc", param.payor_code_mc),
                         new OracleParameter("p_agreement_type_id", param.agreement_type_id),
                         new OracleParameter("p_provider_code", param.provider_code),
                         new OracleParameter("p_service_name", param.service_name),
                         new OracleParameter("p_service_class", param.service_class),
                         new OracleParameter("p_price", param.price),
                         new OracleParameter("p_fixed_price", param.fixed_price),
                         new OracleParameter("p_disc", param.disc),
                         new OracleParameter("p_disc_amount", param.disc_amount),
                         new OracleParameter("p_effective_date", OracleDbType.Date, param.effective_date, System.Data.ParameterDirection.Input),
                         new OracleParameter("p_admedika_detail_service_code", param.admedika_detail_service_code),
                         new OracleParameter("cv_1", OracleDbType.RefCursor, System.Data.ParameterDirection.Output)
                     );

                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void MoveFile(string sourceFile, string destinationFolder)
        {
            try
            {
                // Verify source file exists
                if (!File.Exists(sourceFile))
                {
                    Console.WriteLine("Source file does not exist");
                    return;
                }

                // Create destination directory if it doesn't exist
                Directory.CreateDirectory(destinationFolder);

                string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(sourceFile));

                // Perform the move operation
                File.Move(sourceFile, destinationFile, overwrite: true);
                Console.WriteLine($"Successfully moved file to: {destinationFile}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Permission denied: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving file: {ex.Message}");
            }
        }
    }
}

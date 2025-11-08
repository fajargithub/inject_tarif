using InjectServiceWorker.Models;
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

            //await GetFolderListName(); return;
            //GetAllFileNames(donePath); return;

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
                                if (rowData.Count > 0 && row > 0)
                                {
                                    InjectServiceOCRModel model = new InjectServiceOCRModel();

                                    var arrGroupTarif = rowData[0] != null ? rowData[1].ToString().Split("::") : new string[] { "" };
                                    if (rowData[0] != null)
                                    {
                                        if (arrGroupTarif.Count() == 3 && !string.IsNullOrEmpty(rowData[0].ToString()))
                                        {
                                            model.group_tarif = arrGroupTarif[0];
                                            model.sub_tarif = arrGroupTarif[1];
                                            model.nm_tarif = arrGroupTarif[2];
                                            model.kd_tarif_pro = rowData[0].ToString();
                                            model.hg_jua = rowData[2] != null ? decimal.Parse(rowData[2].ToString()) : 0;
                                            model.disc = rowData[3] != null ? decimal.Parse(rowData[3].ToString()) : 0;
                                            model.disc_rp = rowData[4] != null ? decimal.Parse(rowData[4].ToString()) : 0;
                                            model.effective_date = rowData[16] != null ? DateTime.ParseExact(rowData[16].ToString(), "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture) : null;
                                            model.kd_holding = rowData[17] != null ? rowData[17].ToString() : "";

                                        }
                                    }

                                    //model.kd_tarif = rowData[0] != null ? rowData[0].ToString() : "";
                                    //model.nm_tarif = rowData[1] != null ? rowData[1].ToString() : "";
                                    //model.hg_jua = rowData[2] != null ? Decimal.Parse(rowData[2].ToString()) : 0;
                                    //model.provider_code = rowData[3] != null ? rowData[3].ToString() : "";
                                    //model.kd_holding = rowData[4] != null ? rowData[4].ToString() : "";
                                    //model.item_id = rowData[5] != null ? rowData[5].ToString() : "";
                                    //model.disc = rowData[10] != null ? Decimal.Parse(rowData[10].ToString()) : 0;
                                    //model.disc_rp = rowData[11] != null ? Decimal.Parse(rowData[11].ToString()) : 0;
                                    //model.kd_tarif_payor = rowData[12] != null ? rowData[12].ToString() : "";
                                    //model.nm_tarif_payor = rowData[13] != null ? rowData[13].ToString() : "";
                                    //model.kd_tarif_pro = rowData[14] != null ? rowData[14].ToString() : "";
                                    //model.nm_tarif_pro = rowData[15] != null ? rowData[15].ToString() : "";
                                    //model.efective_date = rowData[16] != null ? DateTime.ParseExact(rowData[16].ToString(), "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture) : null;
                                    //model.hg_jua_new = rowData[17] != null ? Decimal.Parse(rowData[17].ToString()) : 0;
                                    //model.agreement_type = rowData[18] != null ? rowData[18].ToString() : "";

                                    //await InsertTblRateProvider(model);
                                }
                            }
                        }

                        //Move file here
                        MoveFile(filePath, donePath);
                    }
                }

                //await UpdateInjectTblRateProvider();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task InsertTblRateProvider(ServiceRateProviderModel param)
        {
            try
            {
                // Execute the stored procedure
                await _dbContext.Database.ExecuteSqlRawAsync(
                         "BEGIN INSERT_TBL_RATE_PROVIDER(:p_kd_tarif, :p_nm_tarif, :p_hg_jua, :p_provider_code, " +
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

        public async Task UpdateInjectTblRateProvider()
        {
            try
            {
                // Execute the stored procedure
                var result = await _dbContext.Database.ExecuteSqlRawAsync("BEGIN INJECT_TBL_RATE_PROVIDER; END;");
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

        public async Task GetFolderListName()
        {

            // Get all subdirectories in the path
            string path = $@"\\cluster-nas\FTP\FTP\YAKESPENANTAM\SOFTCOPY_DCS\2025\5\28";
            string batchList = string.Empty;
            string[] folders = Directory.GetDirectories(path);

            Console.WriteLine("Folders:");
            foreach (string folder in folders)
            {
                // Get only the folder name, not the full path
                string folderName = Path.GetFileName(folder);
                batchList += "'" + folderName + "'" + ", ";
            }

            var strBatchList = batchList.TrimEnd(',', ' ');
            Console.WriteLine(strBatchList);

            try
            {
                var strquery = "SELECT batch_no, kd_holding from tbl_claim_batch WHERE kd_holding != 'CH0022' AND batch_no in (" + strBatchList + ")";
                var result = await _dbContext.BatchResponses.FromSqlRaw(strquery).AsNoTracking().ToListAsync();
                if (result.Count() > 0)
                {
                    for (int i = 0; i < result.Count(); i++)
                    {
                        string folderPath = path + "\\" + result[i].batch_no;

                        if (Directory.Exists(folderPath))
                        {
                            // Delete folder and all its contents
                            Directory.Delete(folderPath, recursive: true);
                            Console.WriteLine("Folder deleted successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Folder does not exist.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void GetAllFileNames(string folderPath)
        {
            //string folderPath = @"C:\YourFolderPath"; // Replace with your folder path test

            try
            {
                // Get all file names in the directory
                string[] fileNames = Directory.GetFiles(folderPath)
                                            .Select(Path.GetFileName)
                                            .ToArray();

                // Join file names with commas
                string commaSeparated = string.Join(", ", fileNames);

                Console.WriteLine("Files in folder:");
                Console.WriteLine(commaSeparated);
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Directory not found!");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("No permission to access the directory!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using Supplier_AzureTBLRModel.Entities;
using Supplier_AzureTBLRModel.ViewModels;

namespace AddSupplierFunc_AzureTBL
{
    public static class Function1
    {
        [FunctionName("AddSupplier")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string datetimeTick = DateTime.UtcNow.Ticks.ToString();

                /// get storage account
                CloudStorageAccount storageAcc = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=tablestorage1;AccountKey=mvdDaDEswE16qPIWefkIjcpiNjpvC8GbXEglDBjrMKItK0QsFXFxr0SwNSjIdzdKeDrShIZ6abHw+AStfRWs5A==;EndpointSuffix=core.windows.net");

                //// create table client
                CloudTableClient tblclient = storageAcc.CreateCloudTableClient(new TableClientConfiguration());

                // get customer table
                CloudTable cloudTable = tblclient.GetTableReference("Supplier");
                string requestBody;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }

                

                //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                SupplierAddVM data = JsonConvert.DeserializeObject<SupplierAddVM>(requestBody);

                Supplier supplier = new Supplier()
                {
                    Timestamp = DateTime.UtcNow,
                    RowKey = datetimeTick,
                    PartitionKey = datetimeTick,
                    Id = datetimeTick,
                    Address = data.Address,
                    Contact = data.Contact,
                    EmailID = data.EmailID,
                    SupplierName = data.SupplierName,

                };
                
                TableOperation insertOperation = TableOperation.InsertOrMerge(supplier);
                TableResult insertOrMergeTableResult = await cloudTable.ExecuteAsync(insertOperation);

                return new OkObjectResult(new ResponseModel() { Data = supplier, Success = true });
            }
            catch (Exception e)
            {
                return new OkObjectResult(new ResponseModel() { Data = e.Data, Success = false});
            }
        }
    }
}

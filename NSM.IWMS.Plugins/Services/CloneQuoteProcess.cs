using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;


namespace NSM.IWMS.Plugins.Services
{
    public class CloneQuoteProcess
    {
       
        protected ITracingService traceService;
        protected IOrganizationService organizationService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Service"></param>
        /// <param name="trace"></param>
        public CloneQuoteProcess(IOrganizationService Service, ITracingService trace)
        {
            
            organizationService = Service;
            traceService = trace;
        }

        /// <summary>
        /// Create New Quote
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
         public string InitiateCloneProcess(string entityName, Guid entityId)
        {
            Entity sourceQuote = GetSourceQuote(entityId);
            if(sourceQuote != null)
            {
                Entity targetQuote = new Entity(entityName);
                targetQuote = sourceQuote;
                targetQuote.Attributes.Remove("quoteid");
                targetQuote.Id = Guid.Empty;
                targetQuote["vel_sourcequote"] = new EntityReference(entityName, entityId);
                traceService.Trace(string.Format("Info : {0}", "Creating Target Quote"));
                var targetQuoteId =   organizationService.Create(targetQuote);
                traceService.Trace(string.Format("Info : {0} {1}", "Target Quote : ",targetQuoteId));
                CopyQuoteProducts(entityId,targetQuoteId);
                traceService.Trace(string.Format("Info : {0}", "Quote Products Created"));
                return targetQuoteId.ToString();
            }

            else
            {
                throw new Exception("Error - IntitiateCloneProcess - Invalid Source ID" + entityId.ToString());
            }
           
        }

        /// <summary>
        /// For Exisiting Quote, Update values
        /// </summary>
        /// <param name="newQuote"></param>
        /// <exception cref="Exception"></exception>
        public void InitiateCloneProcess(Entity newQuote)
        {

            if(newQuote != null && newQuote.Attributes.Contains("vel_sourcequote"))
            {
                var sourceQuote = newQuote.GetAttributeValue<EntityReference>("vel_sourcequote");
                Entity sourceQuoteEntity = GetSourceQuote(sourceQuote.Id);
                if (sourceQuoteEntity != null)
                {
                    Entity targetQuote = new Entity(newQuote.LogicalName);
                    targetQuote = sourceQuoteEntity;
                    targetQuote["quoteid"]=newQuote["quoteid"];
                    targetQuote.Id = newQuote.Id;
                    targetQuote["opportunityid"] = newQuote["opportunityid"];
                    traceService.Trace(string.Format("Quote ID Logic"));
                    // Quote ID Logic
                    if (!newQuote.Attributes.Contains("name") && newQuote.Attributes.Contains("opportunityid"))
                    {
                        traceService.Trace(string.Format("Quote ID Logic 2"));
                        var quoteId =  GetRFQName(newQuote.GetAttributeValue<EntityReference>("opportunityid").Id) + ".1";
                        targetQuote["name"] = quoteId;

                    }
                    traceService.Trace(string.Format("Info : {0}", "Updating Target Quote"));
                    organizationService.Update(targetQuote);
                    traceService.Trace(string.Format("Info : {0} ", "Target Quote Updated: "));
                    CopyQuoteProducts(sourceQuote.Id, newQuote.Id);
                    traceService.Trace(string.Format("Info : {0}", "Quote Products Created"));
                   // return targetQuoteId.ToString();
                }

                else
                {
                    throw new Exception("Error - IntitiateCloneProcess - Invalid Source ID" );
                }
            }
        }

       private void CopyQuoteProducts( Guid sourceQuoteId, Guid targetQuoteId)
        {
            string fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='quotedetail'>
    <attribute name='productid' />
    <attribute name='productdescription' />
    <attribute name='priceperunit' />
    <attribute name='quantity' />
    <attribute name='extendedamount' />
    <attribute name='quotedetailid' />
    <attribute name='volumediscountamount' />
    <attribute name='uomid' />
    <attribute name='tax' />
    <attribute name='sequencenumber' />
    <attribute name='isproductoverridden' />
    <attribute name='propertyconfigurationstatus' />
    <attribute name='producttypecode' />
    <attribute name='productname' />
    <attribute name='ispriceoverridden' />
    <attribute name='quotedetailname' />
    <attribute name='manualdiscountamount' />
    <attribute name='lineitemnumber' />
    <attribute name='description' />
    <attribute name='transactioncurrencyid' />
    <attribute name='productassociationid' />
    <attribute name='baseamount' />
    <order attribute='productid' descending='false' />
    <filter type='and'>
      <condition attribute='quoteid' operator='eq' value='{sourceQuoteId.ToString()}' />
    </filter>
  </entity>
</fetch>";



            EntityCollection sourceQuoteProducts = organizationService.RetrieveMultiple(new FetchExpression(fetchXml));
            if(sourceQuoteProducts != null && sourceQuoteProducts.Entities.Count > 0)
            {
                foreach(var sourceQuoteProduct in sourceQuoteProducts.Entities)
                {
                    Entity targetQuoteProduct = new Entity(sourceQuoteProduct.LogicalName);
                    targetQuoteProduct =     sourceQuoteProduct;
                    targetQuoteProduct.Id = Guid.Empty;
                    targetQuoteProduct.Attributes.Remove("quotedetailid");
                    targetQuoteProduct["quoteid"] = new EntityReference(targetQuoteProduct.LogicalName, targetQuoteId);
                    organizationService.Create(targetQuoteProduct);

                }
            }
        }
   
        private Entity GetSourceQuote(Guid quoteId)
        {
            string fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='quote'>
   
    <attribute name='customerid' />
 
    <attribute name='totalamount' />
    <attribute name='quoteid' />
    <attribute name='createdon' />
    <attribute name='vel_vendor' />
    <attribute name='vel_taskid' />
    <attribute name='vel_respondbydate' />
    <attribute name='vel_jobid' />
    <attribute name='transactioncurrencyid' />
    <attribute name='totaltax' />
    <attribute name='totallineitemdiscountamount' />
    <attribute name='totallineitemamount' />
    <attribute name='totaldiscountamount' />
    <attribute name='discountpercentage' />
    <attribute name='discountamount' />
   <attribute name='pricelevelid' />

    <order attribute='name' descending='false' />
    <filter type='and'>
      <condition attribute='quoteid' operator='eq' uiname='' uitype='quote' value='{quoteId.ToString()}' />
    </filter>
  </entity>
</fetch>";

            EntityCollection sourceQuote = organizationService.RetrieveMultiple(new FetchExpression(fetchXml));
            if (sourceQuote != null && sourceQuote.Entities.Count > 0)
            {
                traceService.Trace(string.Format("Quote Found {0}", quoteId.ToString()));
                return sourceQuote.Entities[0];
            }
            else
            {
                traceService.Trace(string.Format("Error Occurred : {0} {1}", "Source Quote",quoteId.ToString()));
                traceService.Trace(string.Format("Error Occurred : {0} in {1}", "Source Quote Not Found", "GetSourceQuote"));
                return null;
            }
        }

        private string GetRFQName(Guid rfqId)
        {
            var rfqName = string.Empty;
            if (rfqId != Guid.Empty)
            {
                traceService.Trace(string.Format("GetRFQName"));
                Entity rfq = organizationService.Retrieve("opportunity", rfqId, new ColumnSet(new string[] { "name" }));
                if (rfq != null)
                {
                  
                    rfqName = rfq.Attributes.Contains("name")?rfq.GetAttributeValue<string>("name"):string.Empty;
                    traceService.Trace(string.Format("RFQ ID - {0}", rfqName));
                }
            }
            return rfqName;
        }
    }
}

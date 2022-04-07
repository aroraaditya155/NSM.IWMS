
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using System;

using NSM.IWMS.Plugins.Services;

namespace NSM.IWMS.Plugins
{
    public class CloneQuote : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService organizationService = factory.CreateOrganizationService(context.UserId);

            CloneQuoteProcess cloneQuoteProcess = new CloneQuoteProcess(organizationService, tracer);

        
            try
            {
                if (context.MessageName == "Create")
                {

                }
                else {
                    if (context.InputParameters.Contains("SourceId"))
                    {

                        var quoteId = (string)context.InputParameters["SourceId"];
                        var entityName = "quote";
                       var newQuoteId = cloneQuoteProcess.InitiateCloneProcess(entityName, new Guid( quoteId));
                        context.OutputParameters["QuoteId"] = newQuoteId;
                        //throw new InvalidPluginExecutionException(newQuoteId);

                    }
                }
               
                

            }
            catch (Exception ex)
            {
                tracer.Trace("Error in CloneQuote  Plugin " + ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }

            
        }
    }

}

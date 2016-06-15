using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Observers;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    /*
      The following will be required from this grain.
      1 . Need to observe the changes from the IOrniscientReporting Grain
      2 . Need to maintain state for the current user, so that he/she can view and apply changes to their dashboard.
    */
    //public interface IOrniscientDashboardGrain : IGrainWithStringKey
    //{
    //    Task<List<UpdateModel>> GetCurrentSnapshot(SearchFilter filter);
    //}

    //public class OrniscientDashboardGrain : Grain, IOrniscientDashboardGrain,IOrniscientObserver
    //{
    //    private IOrniscientReportingGrain orniscientReportingGrain;
    //    private List<UpdateModel> filteredStats;
    //    private SearchFilter _filter;

    //    public override Task OnActivateAsync()
    //    {
    //        return base.OnActivateAsync();
    //    }

    //    public async Task<List<UpdateModel>> GetCurrentSnapshot(SearchFilter filter)
    //    {
    //        this._filter = filter;

    //        var reportingGrain = GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
    //        var grains = await reportingGrain.GetAll();

    //        //filter using the user suppplied filter.


    //        return null;
            
    //    }

    //    #region IOrniscientObserver

    //    /// <summary>
    //    ///  This is to get the changes from the orniscient reporting grain.
    //    /// </summary>
    //    /// <param name="model"></param>
    //    public void GrainsUpdated(DiffModel model)
    //    {
    //        //This should filter out the changes, then send the update down to the client, that can not be a singleton....this needs to be per user basis.


    //        throw new System.NotImplementedException();
    //    }

    //    #endregion
    //}

    //public class SearchFilter
    //{
    //    public List<string> Silos { get; set; }
    //    public List<FilterItem> Filters { get; set; }
    //}

    //public class FilterItem
    //{
    //    public string FilterType { get; set; }
    //    public string FilterName { get; set; }
    //    public string[] Values { get; set; }
    //}
}

var Dashboard = React.createClass({
	//React methods
	childContextTypes: {
		selectedFilters: React.PropTypes.object
	},
	getChildContext: function () {
		return {
			selectedFilters: this.state.selectedFilters
		};
	},
	getInitialState: function () {
		return {
			grainIdFilter: '',
			selectedSilos: null,
			selectedTypes: null,
			silos: [],
			availableTypes: [],
			availableFilters: [],
			selectedFilters: {},
			typeCounts: [],
			selectedNodeGrainId: '',
			selectedNodeGrainSilo: '',
			selectedNodeGrainType: '',
			selectedNodeGrainAvailableMethods: [],
			selectedTypeGrainIds: [],
			selectedGrainMethod: null,
			invokedMethodReturn: null,
			invokeGrainMethodOnNewGrain: false,
			selectedNodeGrainKeyType: '',
			grainIdTextInputValue: '',
			disableInvokeMethodButton: false,
			grainInfoLoading: false
		};
	},
	componentWillMount: function () {
		var xhr = new XMLHttpRequest();
		xhr.open('get', orniscienturls.dashboardInfo, true);
		xhr.onload = function () {
			var data = JSON.parse(xhr.responseText);
			this.setState({
				silos: orniscientutils.stringArrToSelectOptions(data.Silos),
				availableTypes: orniscientutils.stringArrToFilterNames(data.AvailableTypes)
			});
		}.bind(this);
		xhr.send();
	},
	componentDidMount: function () {
		window.addEventListener('orniscientUpdated', this.orniscientUpdated);
		orniscient.init();

		window.addEventListener('nodeSelected', this.orniscientNodeSelected);
		window.addEventListener('nodeDeselected', this.resetGrainInformationState);
	},
	//end React methods

	filterByGrainId: function (event) {
		this.setState({
			grainIdFilter: event.target.value
		});
	},
	siloSelected(val) {
		this.setState({
			selectedSilos: val
		});
	},
	getFilters: function (selectedTypes) {
		var requestData = {
			Types: selectedTypes != null && selectedTypes.length > 0 ? selectedTypes.map(function (a) {
				return a.value;
			}) : {}
		};

		var xhr = new XMLHttpRequest();
		xhr.open('post', orniscienturls.getFilters, true);
		xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
		xhr.onload = function () {
			var filters = [];
			if (xhr.responseText != null && xhr.responseText !== '') {
				filters = JSON.parse(xhr.responseText);
			}

			//TODO :  delete all the types from the selectedFilters so that state is not maintained
			this.setState({
				availableFilters: filters,
				selectedTypes: selectedTypes
			});
		}.bind(this);
		xhr.send(JSON.stringify(requestData));
	},
	filterSelected: function (type, filterName, selected) {

		var selectedFilters = this.state.selectedFilters;
		var filterid = filterName.replace(/[^\w]/gi, '.'); //remove special characters

		if (selectedFilters[type] === undefined) {
			selectedFilters[type] = {};
		}

		if (selectedFilters[type][filterid] === undefined) {
			selectedFilters[type][filterid] = {};
		}

		selectedFilters[type][filterid] = selected;
		this.setState({
			selectedFilters: selectedFilters
		});
	},
	searchClicked: function (e) {
		e.preventDefault();
		//build the filter here.
		var selectedFilters = this.state.selectedFilters;
		var filter = {
			GrainId: this.state.grainIdFilter
		}

		if (!orniscientutils.isNullOrUndefined(this.state.selectedSilos)) {
			filter.SelectedSilos = this.state.selectedSilos.map(function (silo) {
				return silo.value;
			});
		}

		if (!orniscientutils.isNullOrUndefined(this.state.selectedTypes)) {
			filter.TypeFilters = this.state.selectedTypes.map(function (type) {
				var selectedValues = {};
				var selectedValuesForType = selectedFilters[type.value];
				for (var key in selectedValuesForType) {
					selectedValues[key] = selectedValuesForType[key].map(function (i) {
						return i.value;
					});
				}
				return {
					TypeName: type.value,
					SelectedValues: selectedValues
				};
			});
		}

		orniscient.getServerData(filter);
	},
	clearFiltersClicked: function (e) {
		e.preventDefault();
		//clear everything that was set with this.setState({})
		this.setState({
			grainIdFilter: '',
			selectedSilos: null,
			selectedTypes: null,
			availableFilters: [],
			selectedFilters: {}
		});

		orniscient.getServerData();
	},
	setSummaryViewLimitClicked: function (e) {
		e.preventDefault();
		//TODO: make this a part of state
		var limit = $('#summaryviewlimit').val();
		if (!orniscientutils.isNullOrUndefined(limit)) {
			var requestData = {
				summaryViewLimit: limit,
				sessionId: orniscient.getSessionId()
			};

			var xhr = new XMLHttpRequest();
			xhr.open('post', 'dashboard/SetSummaryViewLimit', true);
			xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
			xhr.onload = function () {
				this.searchClicked(e);
				this.forceUpdate();
			}.bind(this);
			xhr.send(JSON.stringify(requestData));
		}
	},
	orniscientUpdated: function (typeCounts, a, b) {
		this.setState({
			typeCounts: typeCounts.detail
		});
	},
	resetGrainInformationState: function () {
		this.setState({
			selectedGrainType: null,
			invokedMethodReturn: null,
			selectedGrainMethod: null,
			selectedTypeGrainIds: [],
			selectedNodeGrainAvailableMethods: [],
			invokeGrainMethodOnNewGrain: false,
			selectedNodeGrainId: '',
			selectedNodeGrainSilo: ''
		});
	},
	grainTypeSelected: function (val) {
		this.resetGrainInformationState();
		this.setState({
			selectedGrainType: val
		});
		if (!orniscientutils.isNullOrUndefined(val)) {
			this.getInfoForGrainType(val.value);
		}
	},
	orniscientNodeSelected: function (node) {
		this.resetGrainInformationState();
		this.setState({
			selectedGrainType: node.detail.graintype
		});
		this.getInfoForGrainType(node.detail.graintype);
		this.setState({
			selectedNodeGrainId: node.detail.grainId,
			selectedNodeGrainSilo: node.detail.silo,
			selectedNodeGrainType: node.detail.graintype
		});
	},
	getInfoForGrainType: function (grainType) {
		var requestData = {
			type: grainType
		};
		var xhr = new XMLHttpRequest();
		xhr.open('post', 'dashboard/GetInfoForGrainType', true);
		xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
		xhr.onload = function () {
			var data = JSON.parse(xhr.responseText);
			this.setState({
				selectedNodeGrainAvailableMethods: orniscientutils.methodsToSelectOptions(data.Methods),
				selectedTypeGrainIds: orniscientutils.stringArrToSelectOptions(data.Ids),
				selectedNodeGrainKeyType: data.KeyType,
				grainInfoLoading: false
			});
		}.bind(this);
		xhr.send(JSON.stringify(requestData));
	},
	grainMethodSelected(val) {
		this.setState({
			selectedGrainMethod: val,
			invokedMethodReturn: null
		}, this.disableInvokeMethodButton);
	},
	invokeGrainMethod: function (e) {
		e.preventDefault();
		var methodData = this.state.selectedGrainMethod;
		var parameterValues = [];

		$.each(methodData.parameters, function (index, parameter) {
			var parameterValue = $('#' + parameter.Name).val();
			if (parameterValue !== '') {
				if (parameter.IsComplexType) {
					parameterValues.push({
						'name': parameter.Name,
						'type': parameter.Type,
						'value': JSON.parse(parameterValue)
					});
				} else {
					parameterValues.push({
						'name': parameter.Name,
						'type': parameter.Type,
						'value': JSON.stringify(parameterValue)
					});
				}
			} else {
				parameterValues.push({
					'name': parameter.Name,
					'type': parameter.Type,
					'value': null
				});
			}
		});

		var grainId = this.state.selectedNodeGrainId;
		if (this.state.invokeGrainMethodOnNewGrain) {
			grainId = this.state.grainIdTextInputValue;
		}

		var requestData = {
			type: this.state.selectedNodeGrainType,
			id: grainId,
			methodId: methodData.value,
			parametersJson: JSON.stringify(parameterValues),
			invokeOnNewGrain: this.state.invokeGrainMethodOnNewGrain
		};

		var xhr = new XMLHttpRequest();
		xhr.open('post', 'dashboard/InvokeGrainMethod', true);
		xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
		xhr.onreadystatechange = function () {
			if (xhr.readyState === 4) {
				if (xhr.status === 200) {
					this.setState({
						invokedMethodReturn: xhr.responseText
					});
				} else {
					this.setState({
						invokedMethodReturn: xhr.statusText
					});
				}
			}
		}.bind(this);
		xhr.send(JSON.stringify(requestData));
	},
	invokeMethodOnNewGrainToggle: function (e) {
		this.setState({
			invokeGrainMethodOnNewGrain: e.target.checked
		}, this.disableInvokeMethodButton);
	},
	grainIdSelectChange: function (val) {
		var value = orniscientutils.isNullOrUndefined(val) ? null : val.value;
		this.setState({
			selectedNodeGrainId: value
		});
	},
	grainIdTextInputChange: function (e) {
		this.setState({
			grainIdTextInputValue: e.target.value
		}, this.disableInvokeMethodButton);
	},
	disableInvokeMethodButton: function () {
		this.setState({
			disableInvokeMethodButton: false
		});
		if (this.state.invokeGrainMethodOnNewGrain && this.state.selectedNodeGrainKeyType !== "System.Guid" && !this.state.grainIdTextInputValue) {
			this.setState({
				disableInvokeMethodButton: true
			});
		}
	},
    render: function () {
        return (
			<div id="filterwrap">
				<div className="container bigContainer ">
					<div className="row">
						<div id="mynetwork"></div>
					</div>
				</div>
				<div className="flyout filterFlyout">
					<div className="container">
						<div className="floatButton">
							<button className="btn btn-default toggleFlyout"><span className="glyphicon glyphicon-chevron-right"></span></button>
						</div>
						<div className="row">
							<div className="col-md-12">
								<h4>Filters</h4>
								<form>
									<div className="form-group">
										<label for="grainid">Grain Id</label>
										<input type="text" className="form-control width100" id="grainid" placeholder="Grain Id" onChange={this.filterByGrainId} value={this.state.grainIdFilter} />
									</div>
									<div className="form-group">
										<label for="silo">Silo</label>
										<Select name="form-field-name" options={this.state.silos} multi={true} onChange={this.siloSelected} disabled={false} value={this.state.selectedSilos} />
									</div>
									<div className="form-group">
										<label for="grainType">Grain Type</label>
										<Select name="form-field-name" options={this.state.availableTypes} multi={true} onChange={this.getFilters} disabled={false} value={this.state.selectedTypes} />
									</div>
									<DashboardTypeFilterList data={this.state.availableFilters} filterSelected={this.filterSelected} />
									<div className="row">
										<div className="col-md-12">
											<button type="submit" className="btn btn-success pull-right" onClick={this.searchClicked}>Search</button>
											<button type="submit" className="btn btn-danger pull-left" onClick={this.clearFiltersClicked}>Clear Filters</button>
										</div>
									</div>
									<DashboardTypeCounts data={this.state.typeCounts} />
									<div id="summaryViewLimit">
										<h4>Summary View Limit</h4>
										<div className="form-group">
											<input type="text" className="form-control" id="summaryviewlimit" placeholder="Summary View Limit" />

										</div><button type="submit" className="btn btn-success pull-right" onClick={this.setSummaryViewLimitClicked}>Set Limit</button>
									</div>
								</form>
							</div>
						</div>
					</div>
				</div>
				<div className="flyout grainFlyout">
					<div className="container">
						<div className="floatButton">
							<button className="btn btn-default toggleFlyout"><span className="glyphicon glyphicon-chevron-left"></span></button>
						</div>
        				{orniscient.summaryView() === false &&
						<div className="row">
							<div className="col-md-12">
								<h4>Grain Method Invocation</h4>
								<form>
									<div className="form-group">
										<h5>Grain Type</h5>
										<Select name="form-field-name" options={this.state.availableTypes} multi={false} onChange={this.grainTypeSelected} value={this.state.selectedGrainType} />
									</div>
									<div className="form-group">
										<h5>Grain Methods</h5>
										<Select name="form-field-name" options={this.state.selectedNodeGrainAvailableMethods} multi={false} onChange={this.grainMethodSelected} value={this.state.selectedGrainMethod} />
									</div>
                    				{this.state.selectedGrainMethod !== null &&
									<div>
										<div className="form-group" id="parameterInputs">
											<DashboardGrainMethodParameters data={this.state.selectedGrainMethod} />
										</div>
									</div>
                    				}
									<div>
										<h5>Target</h5>
										<div className="form-group">
											<h5>Grain Id</h5>
											<Select name="form-field-name" options={this.state.selectedTypeGrainIds} onChange={this.grainIdSelectChange} multi={false} value={this.state.selectedNodeGrainId} />
										</div>
										<div className="form-group">
											<h5>Silo</h5>
											<label for="silo">{this.state.selectedNodeGrainSilo !== '' ? this.state.selectedNodeGrainSilo : 'No grain selected.'}</label>
										</div>
									</div>
									{this.state.selectedGrainMethod !== null &&
									<div>
										<h5>Method Invocation</h5>
										<table>
											<tr>
												<td><input type="checkbox" id="invokeMethodOnNewGrain" onChange={this.invokeMethodOnNewGrainToggle} /></td>
												<td><label>Invoke method on new grain</label></td>
											</tr>
										</table>
										{this.state.invokeGrainMethodOnNewGrain === true &&
										<div className="form-group">
											<label for="invokeMethodNewGrainId">New Grain Id</label>
											<input type="text" className="form-control width100" id="invokeMethodNewGrainId" placeholder={this.state.selectedNodeGrainKeyType } value={this.state.grainIdTextInputValue} onChange={this.grainIdTextInputChange} disabled={this.state.selectedNodeGrainKeyType==="System.Guid" } />
										</div>
										}
										<div className="row">
											<div className="col-md-12">
												<button type="submit" className="btn btn-success pull-left" id="invokeMethodButton" onClick={this.invokeGrainMethod} disabled={this.state.disableInvokeMethodButton}>Invoke Method </button>
											</div>
										</div>
									</div>
									}
								</form>
								<DashboardGrainMethodReturnData data={this.state.invokedMethodReturn} />
							</div>
						</div>
        				} {orniscient.summaryView() === true &&
						<div className="alert alert-info" role="alert">
							<strong>Method invocation is diasbled during summary view.</strong>
						</div>
        				}
					</div>
    				{this.state.grainInfoLoading &&
					<div className="loader"></div>
    				}
				</div>
			</div>
        );
    }
});

ReactDOM.render(
    <Dashboard />,
    document.getElementById('dashboard')
);

$(document).ready(function () {
	$(document).on('click', '.toggleFlyout', function (e) {
		e.preventDefault();

		if ($(this).closest('.flyout').hasClass('filterFlyout')) {
			var $filterLayout = $('.filterFlyout');
			$filterLayout.toggleClass('shown');

			if ($filterLayout.hasClass('shown')) {
				$filterLayout.find('.glyphicon').removeClass('glyphicon-chevron-right').addClass('glyphicon-chevron-left');
			} else {
				$filterLayout.find('.glyphicon').removeClass('glyphicon-chevron-left').addClass('glyphicon-chevron-right');
			}
		} else {
			var $grainFlyout = $('.grainFlyout');
			$grainFlyout.toggleClass('shown');

			if ($grainFlyout.hasClass('shown')) {
				$grainFlyout.find('.glyphicon').removeClass('glyphicon-chevron-left').addClass('glyphicon-chevron-right');
			} else {
				$grainFlyout.find('.glyphicon').removeClass('glyphicon-chevron-right').addClass('glyphicon-chevron-left');
			}
		}
	});
});
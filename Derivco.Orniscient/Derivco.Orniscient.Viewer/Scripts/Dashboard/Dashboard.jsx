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
			//Textbox inputs
			grainIdTextInputValue: '',
			grainIdFilterTextInputValue: '',
			summaryViewLimitTextInputValue: '',
			//Selects
			silos: [],
			availableGrainTypes: [],
			availableGrainTypeFilters: [],
			grainTypeCounts: [],
			availableMethodsForGrainType: [],
			selectedGrainTypeGrainIds: [],
			//Selected values
			selectedSilos: null,
			selectedTypes: null,
			selectedFilters: {},
			selectedGrainType: null,
			targetGrainId: '',
			targetGrainSilo: '',
			selectedGrainMethod: null,
			selectedGrainTypeKeyType: '',

			invokedMethodResult: null,
			invokeMethodOnNewGrain: false,
			disableInvokeMethodButton: false,
			grainMethodInvocationDataLoading: false
		};
	},
	componentWillMount: function () {
		var xhr = new XMLHttpRequest();
		xhr.open('get', orniscienturls.dashboardInfo, true);
		xhr.onload = function () {
			var data = JSON.parse(xhr.responseText);
			this.setState({
				silos: orniscientutils.stringArrToSelectOptions(data.Silos),
				availableGrainTypes: orniscientutils.stringArrToFilterNames(data.AvailableTypes)
			});
		}.bind(this);
		xhr.send();
	},
	componentDidMount: function () {
		window.addEventListener('orniscientUpdated', this.orniscientUpdated);
		orniscient.init();

		window.addEventListener('nodeSelected', this.orniscientNodeSelected);
		window.addEventListener('nodeDeselected', this.resetGrainMethodInvocationPanel);
	},
	//end React methods

	//Populate selects
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
				availableGrainTypeFilters: filters,
				selectedTypes: selectedTypes
			});
		}.bind(this);
		xhr.send(JSON.stringify(requestData));
	},
	getInfoForGrainType: function (grainType) {
		var requestData = {
			type: grainType
		};
		var xhr = new XMLHttpRequest();
		xhr.open('post', orniscienturls.getInfoForGrainType, true);
		xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
		xhr.onload = function () {
			var data = JSON.parse(xhr.responseText);
			this.setState({
				availableMethodsForGrainType: orniscientutils.methodsToSelectOptions(data.Methods),
				selectedGrainTypeGrainIds: orniscientutils.stringArrToSelectOptions(data.Ids),
				selectedGrainTypeKeyType: data.KeyType
			});
		}.bind(this);
		xhr.send(JSON.stringify(requestData));
	},
	//Event handler
	orniscientUpdated: function (grainTypeCounts) {
		this.setState({
			grainTypeCounts: grainTypeCounts.detail
		});
	},
	//OnChange
	grainIdFilterTextInputChanged: function (e) {
		this.setState({
			grainIdFilterTextInputValue: e.target.value
		});
	},
	grainIdSelectChanged: function (grainId) {
		var value = orniscientutils.isNullOrUndefined(grainId) ? null : grainId.value;
		this.setState({
			targetGrainId: value
		}, this.disableInvokeMethodButton);
	},
	grainIdTextInputChanged: function (e) {
		this.setState({
			grainIdTextInputValue: e.target.value
		}, this.disableInvokeMethodButton);
	},
	setSummaryViewTextInputChanged: function (e) {
		this.setState({
			summaryViewLimitTextInputValue: e.target.value
		});
	},
	grainMethodSelectionChanged(method) {
		this.setState({
			selectedGrainMethod: method,
			invokedMethodResult: null
		}, this.disableInvokeMethodButton);
	},
	grainTypeSelectionChanged: function (grainType) {
		this.resetGrainMethodInvocationPanel();
		this.setState({
			selectedGrainType: grainType.value
		});
		if (!orniscientutils.isNullOrUndefined(grainType)) {
			this.getInfoForGrainType(grainType.value);
		}
	},
	siloSelectionChanged(silo) {
		this.setState({
			selectedSilos: silo
		});
	},
	invokeMethodOnNewGrainOptionChanged: function (e) {
		this.setState({
			invokeMethodOnNewGrain: e.target.checked
		}, this.disableInvokeMethodButton);
	},
	//Selected
	orniscientNodeSelected: function (node) {
		this.resetGrainMethodInvocationPanel();
		this.getInfoForGrainType(node.detail.graintype);
		this.setState({
			targetGrainId: node.detail.grainId,
			targetGrainSilo: node.detail.silo,
			selectedGrainType: node.detail.graintype
		});
	},
	filterSelected: function (type, filterName, selected) {
		var selectedFilters = this.state.selectedFilters;
		var filterid = filterName.replace(/[^\w]/gi, '.'); //remove special characters

		if (orniscientutils.isNullOrUndefined(selectedFilters[type])) {
			selectedFilters[type] = {};
		}

		if (orniscientutils.isNullOrUndefined(selectedFilters[type][filterid])) {
			selectedFilters[type][filterid] = {};
		}

		selectedFilters[type][filterid] = selected;
		this.setState({
			selectedFilters: selectedFilters
		});
	},
	//Click
	searchButtonClicked: function (e) {
		e.preventDefault();
		//build the filter here.
		var selectedFilters = this.state.selectedFilters;
		var filter = {
			GrainId: this.state.grainIdFilterTextInputValue
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
					if (!orniscientutils.isNullOrUndefined(selectedValuesForType[key])) {
						selectedValues[key] = selectedValuesForType[key].map(function(i) {
							return i.value;
						});
					}
				}
				return {
					TypeName: type.value,
					SelectedValues: selectedValues
				};
			});
		}

		orniscient.getServerData(filter);
	},
	clearFiltersButtonClicked: function (e) {
		e.preventDefault();
		//clear everything that was set with this.setState({})
		this.setState({
			grainIdFilterTextInputValue: '',
			selectedSilos: null,
			selectedTypes: null,
			availableGrainTypeFilters: [],
			selectedFilters: {}
		});

		orniscient.getServerData();
	},
	setSummaryViewLimitButtonClicked: function (e) {
		e.preventDefault();
		var limit = this.state.summaryViewLimitTextInputValue;
		if (!orniscientutils.isNullOrUndefined(limit)) {
			var requestData = {
				summaryViewLimit: limit,
				sessionId: orniscient.getSessionId()
			};

			var xhr = new XMLHttpRequest();
			xhr.open('post', orniscienturls.setSummaryViewLimit, true);
			xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
			xhr.onload = function () {
				this.searchButtonClicked(e);
				this.forceUpdate();
			}.bind(this);
			xhr.send(JSON.stringify(requestData));
		}
	},
	invokeGrainMethodButtonClicked: function (e) {
		e.preventDefault();
		var methodData = this.state.selectedGrainMethod;
		var parameterValues = [];
		this.setState({
			disableInvokeMethodButton: true
		});
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
		var grainId = this.state.targetGrainId;
		if (this.state.invokeMethodOnNewGrain) {
			grainId = this.state.grainIdTextInputValue;
		}
		var requestData = {
			type: this.state.selectedGrainType,
			id: grainId,
			methodId: methodData.value,
			parametersJson: JSON.stringify(parameterValues),
			invokeOnNewGrain: this.state.invokeMethodOnNewGrain
		};
		var xhr = new XMLHttpRequest();
		xhr.open('post', orniscienturls.invokeGrainMethod, true);
		xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
		xhr.onreadystatechange = function () {
			if (xhr.readyState === 4) {
				this.setState({
					disableInvokeMethodButton: false
				});
				if (xhr.status === 200) {
					this.setState({
						invokedMethodResult: xhr.responseText
					});
				} else {
					this.setState({
						invokedMethodResult: xhr.statusText
					});
				}
			}
		}.bind(this);
		xhr.send(JSON.stringify(requestData));
	},
	//Misc
	resetGrainMethodInvocationPanel: function () {
		this.setState({
			selectedGrainType: null,
			invokedMethodResult: null,
			selectedGrainMethod: null,
			selectedGrainTypeGrainIds: [],
			availableMethodsForGrainType: [],
			invokeMethodOnNewGrain: false,
			targetGrainId: '',
			targetGrainSilo: ''
		});
	},
	disableInvokeMethodButton: function () {
		this.setState({
			disableInvokeMethodButton: false
		});
		if ((orniscientutils.isNullOrUndefinedOrEmpty(this.state.selectedGrainTypeGrainIds) && orniscientutils.isNullOrUndefinedOrEmpty(this.state.grainIdTextInputValue)) ||
				(this.state.invokeMethodOnNewGrain && this.state.selectedGrainTypeKeyType !== 'System.Guid' && !this.state.grainIdTextInputValue) || 
				(!this.state.invokeMethodOnNewGrain && orniscientutils.isNullOrUndefinedOrEmpty(this.state.targetGrainId))) {
			this.setState({
				disableInvokeMethodButton: true
			});
		}
	},

    render: function () {
        return (
			<div id="filterwrap" style={{height: "100%"}}>
				<div className="container bigContainer " style={{height: "100%"}}>
					<div className="row" style={{height: "100%"}}>
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
										<input type="text" className="form-control width100" id="grainid" placeholder="Grain Id" onChange={this.grainIdFilterTextInputChanged} value={this.state.grainIdFilterTextInputValue} />
									</div>
									<div className="form-group">
										<label for="silo">Silo</label>
										<Select name="form-field-name" options={this.state.silos} multi={true} onChange={this.siloSelectionChanged} disabled={false} value={this.state.selectedSilos} />
									</div>
									<div className="form-group">
										<label for="grainType">Grain Type</label>
										<Select name="form-field-name" options={this.state.availableGrainTypes} multi={true} onChange={this.getFilters} disabled={false} value={this.state.selectedTypes} />
									</div>
									<DashboardTypeFilterList data={this.state.availableGrainTypeFilters} filterSelected={this.filterSelected} />
									<div className="row">
										<div className="col-md-12">
											<button type="submit" className="btn btn-success pull-right" onClick={this.searchButtonClicked}>Search</button>
											<button type="submit" className="btn btn-danger pull-left" onClick={this.clearFiltersButtonClicked}>Clear Filters</button>
										</div>
									</div>
									<DashboardTypeCounts data={this.state.grainTypeCounts} />
									<div id="summaryViewLimit">
										<h4>Summary View Limit</h4>
										<div className="form-group">
											<input type="text" className="form-control" id="summaryviewlimit" onChange={this.setSummaryViewTextInputChanged} value={this.state.summaryViewLimitTextInputValue} placeholder="Summary View Limit" />
										</div>
										<button type="submit" className="btn btn-success pull-right" onClick={this.setSummaryViewLimitButtonClicked}>Set Limit</button>
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
										<Select name="form-field-name" options={this.state.availableGrainTypes} multi={false} onChange={this.grainTypeSelectionChanged} value={this.state.selectedGrainType} />
									</div>
									{orniscientvalues.allowMethodsInvocation === 'True' ?
									(<div>
										<div className={orniscientutils.isNullOrUndefinedOrEmpty(this.state.selectedGrainType) ? 'form-group disabledContainer' : 'form-group'}>
											<h5>Grain Methods</h5>
											<Select name="form-field-name" options={this.state.availableMethodsForGrainType} multi={false} onChange={this.grainMethodSelectionChanged} value={this.state.selectedGrainMethod} disabled={orniscientutils.isNullOrUndefinedOrEmpty(this.state.selectedGrainType)} />
										</div>
									</div>) :
									(
									<div className="alert alert-warning">
										<strong>Warning:</strong> Method Invocation disabled.
									</div>)
									}
                    				{this.state.selectedGrainMethod !== null &&
									<div>
										<div className="form-group" id="parameterInputs">
											<DashboardGrainMethodParameters data={this.state.selectedGrainMethod} />
										</div>
										<div className="form-group invokeMethodOnNewGrainCheckbox">
											<label>
												<input type="checkbox" id="invokeMethodOnNewGrain" onChange={this.invokeMethodOnNewGrainOptionChanged} />
												Invoke method on new grain
											</label>
										</div>
									</div>
                    				}
									<div className={orniscientutils.isNullOrUndefinedOrEmpty(this.state.selectedGrainType) ? ' disabledContainer' : ''}>
										<h5>Target Grain</h5>
										<div className="form-group">
											<h5>Grain Id</h5>
											{this.state.invokeMethodOnNewGrain === false &&
											<Select name="form-field-name" options={this.state.selectedGrainTypeGrainIds} onChange={this.grainIdSelectChanged} multi={false} value={this.state.targetGrainId} disabled={orniscientutils.isNullOrUndefinedOrEmpty(this.state.selectedGrainType)} />
											}
											{this.state.invokeMethodOnNewGrain === true &&
											<input type="text" className="form-control width100" id="invokeMethodNewGrainId" placeholder={orniscientutils.isNullOrUndefinedOrEmpty(this.state.selectedGrainMethod) ? 'No Grain Method Selected' : this.state.selectedGrainTypeKeyType} value={this.state.grainIdTextInputValue} onChange={this.grainIdTextInputChanged} disabled={this.state.selectedGrainTypeKeyType === 'System.Guid' || orniscientutils.isNullOrUndefinedOrEmpty(this.state.selectedGrainMethod)} />
											}
										</div>
										<div className="form-group">
											<h5>Silo</h5>
											<label for="silo">{this.state.targetGrainSilo !== '' ? this.state.targetGrainSilo : 'No grain selected.'}</label>
										</div>
									</div>
									{this.state.selectedGrainMethod !== null &&
									<div>
										<div className="row">
											<div className="col-md-12">
												<button type="submit" className="btn btn-success pull-left" id="invokeMethodButton" onClick={this.invokeGrainMethodButtonClicked} disabled={this.state.disableInvokeMethodButton}>Invoke Method </button>
											</div>
										</div>
									</div>
									}
								</form>
								<DashboardGrainMethodReturnData data={this.state.invokedMethodResult} />
							</div>
						</div>
        				} {orniscient.summaryView() === true &&
						<div className="alert alert-info" role="alert">
							<strong>Method invocation is diasbled during summary view.</strong>
						</div>
        				}
					</div>
    				{this.state.grainMethodInvocationDataLoading &&
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
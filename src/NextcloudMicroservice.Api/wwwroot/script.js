const response = fetch(OC.generateUrl('/apps/app_api/proxy/dotnet_microservice/weatherforecast'));
response.then(response => {
    return response.json();
}).then(weatherforecasts => {
    document.getElementById('content').innerHTML = `<div id="app-navigation">
    </div>
    <div id="app-content" class="app-dotnet_microservice">
        <p>
            Number of forecasts: ${weatherforecasts.length}
        </p>
    </div>`;
});

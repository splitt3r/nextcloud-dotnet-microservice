const response = await fetch(OC.generateUrl('/apps/app_api/proxy/dotnet_microservice/weatherforecast'));
const weatherforecast = await response.json();
OCDialogs.alert("Response from Microservice", `Number of forecasts: ${weatherforecast.length}`);

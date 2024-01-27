const response = fetch(OC.generateUrl('/apps/app_api/proxy/dotnet_microservice/weatherforecast'));
response.then(response => {
    return response.json();
}).then(weatherforecasts => {
    document.getElementById('content').innerHTML = `<div id="app-navigation">
        <nav id="app-navigation-test">
            <ul>
                <li class="app-navigation-entry active">
                    <a href="#">
                        <span class="icon-home"></span>
                        <span>Home</span>
                    </a>
                </li>
            </ul>
        </nav>
    </div>
    <div id="app-content" class="app-dotnet_microservice">
        <div id="app-content-wrapper">
            <div class="app-content-list">
                <a href="#" class="app-content-list-item">
                    <div class="app-content-list-item-icon" style="background-color: rgb(31, 192, 216);">M</div>
                    <div class="app-content-list-item-line-one">Important mail is very important! Don't ignore me</div>
                    <div class="app-content-list-item-line-two">Hello there, here is an important mail from your mom</div>
                </a>
                </a>
            </div>
            <div class="app-content-detail">
                Number of forecasts: ${weatherforecasts.length}
            </div>
        </div>
    </div>`;
});

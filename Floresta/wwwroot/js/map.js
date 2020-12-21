﻿var markers = [];

$.ajax({
    type: "GET",
    url: "Map/GetMarkers",
    contentType: "application/json",
    dataType: "json",
    success: function (result) {
        markers = result;
    },
    error: function (xhr, status, error) {
        var errorMessage = xhr.status + ': ' + xhr.statusText
        alert('Error - ' + errorMessage);
    }
})

$.ajax({
    type: "GET",
    url: "Map/GetSeedlings",
    contentType: "application/json",
    dataType: "json",
    success: function (result) {
        seedlings = result;
    },
    error: function (xhr, status, error) {
        var errorMessage = xhr.status + ': ' + xhr.statusText
        alert('Error - ' + errorMessage);
    }
})

function initMap() {
    var uluru = { lat: 48.5405822, lng: 24.9988393 };

    var map = new google.maps.Map(document.getElementById('map'), {
        zoom: 12,
        center: uluru,
        zoomControlOption: {
            position: google.maps.ControlPosition.LEFT_BOTTOM
        },
    });

    var input = document.getElementById('pac-input');
    var dropMarkers = document.getElementById("dropMarkers");
    var searchBox = new google.maps.places.SearchBox(input);


    //Pushing control on the map
    map.controls[google.maps.ControlPosition.TOP_CENTER].push(input);
    map.controls[google.maps.ControlPosition.TOP_CENTER].push(dropMarkers);
    var searchMarkers = [];
    // Listern for the event fired when the user selecets a predition and retrieve more details
    searchBox.addListener('places_changed', function () {
        var places = searchBox.getPlaces();
        if (places.length == 0) {
            return;
        }
        //clear out the old markers
        searchMarkers.forEach(function (marker) {
            marker.setMap(null);
        });
        searchMarkers = [];
        //for each place, get the icon, name and location.
        var bounds = new google.maps.LatLngBounds();
        //debugger
        places.forEach(function (place) {
            if (!place.geometry) {
                console.log("Returned place contains no geometry");
                return;
            }
            var icon = {
                url: place.icon,
                size: new google.maps.Size(71, 71),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(17, 34),
                scaledSize: new google.maps.Size(25, 25),
            };
            //creates a marker for each place
            searchMarkers.push(new google.maps.Marker({
                map: map,
                icon: icon,
                title: place.name,
                position: place.geometry.location
            }));

            if (place.geometry.viewport) {
                bounds.union(place.geometry.viewport);
            }
            else {
                bounds.extend(place.geometry.location);
            }
        });
        map.fitBounds(bounds);

    });

    ///get markers from database
    dropMarkers.addEventListener("click", drop);
    function drop() {
        for (let i = 0; i < markers.length; ++i) {
            const marker = new google.maps.Marker({
                position: {
                    lat: parseFloat(markers[i].lat),
                    lng: parseFloat(markers[i].lng),
                },
                map: map,
                title: markers[i].title,
                plantCount: markers[i].plantCount,
                id: markers[i].id,
                animation: google.maps.Animation.DROP,
            });
            marker.addListener("click", () => {
                if (marker.getAnimation() !== null) {
                    marker.setAnimation(null);
                }
                else {
                    marker.setAnimation(google.maps.Animation.BOUNCE);
                }
            });
            info(marker, markers[i].title);
        }
    }
    function info(marker, title) {
        const infowindow = new google.maps.InfoWindow({
            content: title,
        });
        marker.addListener("click", () => {

            infowindow.open(marker.get("map"), marker);
            $("#markerIdInput").val(marker.id);
            $("#markerTitleInput").val(marker.title);

            var seedlingId = $("#seedlingsDropdown option:selected").val();

            var seedling;

            for (var i = 0; i < seedlings.length; i++) {
                if (seedlings[i].id == seedlingId) {
                    seedling = seedlings[i];
                }
                break;
            }
            var count;
            if (seedling.amount >= marker.plantCount) {
                count = marker.plantCount;
            }
            else if (marker.plantCount > seedling.amount) {
                count = seedling.amount;
            }

            $("#plantCountInput").attr({
                "max": count,
                "min": 1
            });
        });
    }
}
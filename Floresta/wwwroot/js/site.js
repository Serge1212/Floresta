﻿var menu = document.querySelector('.nav_menu'),
    icon = document.querySelector('.animated_menu_icon'),
    menuButton = document.querySelector('#menu_button_container'),
    open= false;

menuButton.onclick = function () {
    if (open == false) {
        icon.classList.add('menu-opened');
        menu.classList.add('menu-opened');
        open = !open;
    }
    else {
        menu.classList.add('menu-closed');
        icon.classList.remove('menu-opened');
        menu.classList.remove('menu-opened');
        setTimeout(closeMenu, 490) 
    }
    function closeMenu() {
        menu.classList.remove('menu-closed');
        open = !open;
    }   
}

var treesAndUsers = [];
$.ajax({
    type: "GET",
    url: "Home/GetDataForChart",
    contentType: "application/json",
    dataType: "json",
    async: false,
    success: function (result) {
        treesAndUsers = result;
    },
    error: function (xhr, status, error) {
        var errorMessage = xhr.status + ': ' + xhr.statusText
        alert('Error - ' + errorMessage);
    }
})

document.getElementById("usersSupportedDiv").innerHTML += ` ${treesAndUsers.users} людей підтримало`;
document.getElementById("treesPlantedDiv").innerHTML += ` ${treesAndUsers.trees} дерев посаджено`;

google.charts.load("current", { packages: ["corechart"] });
google.charts.setOnLoadCallback(drawChart);

function drawChart() {

    var data = google.visualization.arrayToDataTable([
        ['Trees', 'Plant'],
        ['Посаджено', parseInt(treesAndUsers.trees)],
        ['Залишилось посадити', parseInt(treesAndUsers.remainingTrees)]     
    ]);

    var options = {
        colors: ['819F00', 'ABCD2D'],
        pieSliceTextStyle: {
            color: 'white',
        },
        tooltip: null,
        fontSize: 16,
        legend: 'none',
        pieSliceText: 'value',
        backgroundColor: 'none',
        chartArea: {left:20,top:0,width:'100%',height:'100%'},
    };

    var chart = new google.visualization.PieChart(document.getElementById('donut_single'));
    chart.draw(data, options);
}

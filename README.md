# HTCPCP-SERVER
This repository contains a HTCPCP-Compliant Server written in C#. It supports multiple coffeepots (although this configuration option is not yet implemented) on any GPIO-Capable Device (e.g. Raspberry PI). If you do not want to use a GPIO-capable device, the needed controller class should be trivial to add by exchanging the created CoffeeMaker in Program.cs with another implementation of ICoffeeMaker.cs (e.g. by using an Arduino connected to a serial port / sending an http(s)-request to a coffee machine with a remote-control feature and reverse-engineering their API).

The Server supports the use of additions (although the standard GPIOFilterCoffeeMaker cannot handle them -> my filter coffee machine doesn't have any possible mechanisms to add milk or sugar). The number of available coffees (number of times the machine is able to run/produce coffee) and the available amount of additions is stored in a SQLite Database. This can be pre-loaded with values in the following format:

File: input.xml (any other name is possible if specified via the file flag -f)

<?xml version="1.0" encoding="UTF-8"?>
<options>
    <additions pot="pot-0">
        <addition type="Milk/Cream">0</addition>
        <addition type="Milk/Whole-Milk">0</addition>
        <addition type="Milk/Half-and-Half">0</addition>
        <addition type="Milk/Part-Skim">0</addition>
        <addition type="Milk/Skim">0</addition>
        <addition type="Milk/Non-dairy">0</addition>
        <addition type="Syrup/Almond">0</addition>
        <addition type="Syrup/Vanilla">0</addition>
        <addition type="Syrup/Raspberry">0</addition>
        <addition type="Syrup/Chocolate">0</addition>
        <addition type="Alcohol/Whisky">0</addition>
        <addition type="Alcohol/Rum">0</addition>
        <addition type="Alcohol/Kahlua">0</addition>
        <addition type="Alcohol/Aquavit">0</addition>
		<coffee>1</coffee>
    </additions>
</options>

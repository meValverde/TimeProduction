# TimeProduction
This is a REST api, where you can see the energy production for a selected date in 2021.
For now, you can input a date into the API, to see the energy production from a choosen day or period of time 2021.

Calls:
EnergyProduction: Insert a date in 2021 with the format yyMMddHH to get the total of energy produced in that hour.

EnergyProduction {date}: Same input as before, here you will see the energy production per minute within an hour.

EnergyProduction/get period: Insert a start date and a end date, so you can see the total of energy produced within that period. The format must be yyyy-MM-dd HH (Hour is optional). Examples:

2021-04-27 11:00
2021-04-27 11
2021-04-27
EnergyProduction/Location: Get the Forecast for the next 24hrs in a selected area.

Once updated solar energy data is available, will the 3 first functions only get recent data, insted for a user input.

To-do: Razor front end to recieve input and show data.![Dashboard](https://user-images.githubusercontent.com/90765194/160301012-07f940f8-4e0e-44ca-9ac0-6cf82ea54251.png)

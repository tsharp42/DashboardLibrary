#DashboardLibrary
An abstraction layer and associated test application to send vehicle information from racing/driving sims to a hardware dashboard mimicing that of a real vehicle.

##DashboardLibrary
DashboardLibrary is an abstraction layer that sits between the API used to gather the information from the game and the rest of the code that sends that uses the information

##DashboardSender
DashboardSender is a test application that interacts with external hardware over a serial port. 
This application uses CmdMessenger: https://github.com/dreamcat4/CmdMessenger

##Providers
Providers are the glue between the abstraction layer and the game, They must implement the interface "IDashboardDataProvider" found in the DashboardLibrary.

###ETS2DashboardProvider
A Data provider that hooks into Euro Truck Simulator 2 using the fantastic Telementry SDK by nlhans:
https://github.com/nlhans/ets2-sdk-plugin

The Library's data structure is currently being modelled around this provider.

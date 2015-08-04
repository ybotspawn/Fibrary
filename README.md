# Fibrary

This project is an ongoing F# project to replace a legacy multifunction C# class library.

The preexisting C# library was specifically geared to the management of enterprise entities within an Active Directory environment.  This library is being designed to perform the same functions as well as contains the following other entity abstractions:

Database (MySql, SQLServer, possibly more)
Computer management (pingable, remote wmi access, possibly Linux functions)
Network management and enumeration
Email relay interface
LDAP (ActiveDirectory, OpenLdap)
Event log access/manipulation (Windows Event Viewer, SYSLOG)

Currently fActiveDirectory contains the ability/has examples to:
get last logon (abstracted across all domain controllers)
get all users
get lastLogonTimeStamp
get account Disabled
get all domain controllers

Many of these methods work in testing, I have not yet had the opportunity to integrate these into a tested functional library. Much saw dust and work to do

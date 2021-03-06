# PassportLogin
Sample code snippets from Windows Hello documentation put together into a full working project.

## Introduction

Demonstrates using Windows Hello to securely store user credentials and subsequent retrieval of it
using one of the Hello authentication methods. If Windows Hello is not installed or not enabled,
application will revert to traditional login using username and password. A message will also be
displayed that Passport login is disabled as it's not available.

This project is different from the Windows Hello sample code in the various projects under
https://github.com/Microsoft/Windows-universal-samples.git in that, it does not require a separate
server process to be launched. Instead this project incorporates a mock server that stores user
particulars in an in-memory database.

## Login Details

Username and password are hardcoded in the code and the backend authentication is done through a
mock server.

 - Username: sampleUsernmae
 - Password: samplePassowrd

Additional accounts can be registered, details of which are stored in the in-memory database. Further
to registration the accounts can be logged into using Hello login mechanisms. Being an in-memory DB,
the registration details are not preserved across application run sessions.

## Requirements

 - Visual Studio 2017 Community Edition or above
 - C# development environment

## References
The project code is based on snippets in the following tutorial.

 - https://docs.microsoft.com/en-us/windows/uwp/security/microsoft-passport-login
 - https://docs.microsoft.com/en-us/windows/uwp/security/microsoft-passport-login-auth-service

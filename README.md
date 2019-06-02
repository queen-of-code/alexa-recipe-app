# Introduction 
This is a simple dotnet core demo project for standing up a website / API / Database using Docker containers and docker-compose for local development. 

# Getting Started
1.  Install Docker for Windows/Mac/WhateverYourOSIs
2.  Install dotnet core 2.2 at a minimum 
3.  Something to edit C# in (VS Code or VS Community are good choices).

# Build and Test
From within the RecipeApp folder, you can build each component independently, or merely run 'docker-compose build' to compile all dependencies and create the requisite images.
Then you can run 'docker-compose up' to run the services. 

In order to run the application, you'll need to ensure that all the secrets exist (as text files) in your home directory. For example, `~/.docker/secrets/Authentication.Microsoft.ApplicationId.txt` should exist. It doesn't matter what's in the file for most purposes, since you will not be testing with Microsoft or Facebook accounts.

# Running
By default, the application runs on the following ports:
Website - http://localhost:5000
API - http://localhost:8080
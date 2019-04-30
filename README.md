# Introduction 
This is a simple dotnet core demo project for standing up a website / API / Database using Docker containers and docker-compose for local development. 

# Getting Started
1.  Install Docker for Windows/Mac/WhateverYourOSIs
2.  Install dotnet core 2.1 at a minimum 
3.  Something to edit C# in (VS Code or VS Community are good choices).


# Build and Test
From within the AlexaAPI folder, you can build each component independently, or merely run 'docker-compose build' to compile all dependencies and create the requisite images.
Then you can run 'docker-compose up' to run the services.

# MedicalPractitionerPortal

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 17.3.2.
See /ReadMe.md in the root folder for general instructions and how to use chefs form. This ReadMe.md contains commands to run, build, test, and serve the frontend

## Install Packages

Run `npm i` to install packages on first run. You will also need install packages on 'shared-portal-ui', run `npm i` in folder /shared-portal-ui/src

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via a platform of your choice. To use this command, you need to first add a package that implements end-to-end testing capabilities.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.io/cli) page.

## Docker build

change directory to root folder
`docker build --file ./medical-portal/src/UI/Dockerfile . --tag medical-portal-ui`
`docker run -p 4200:4200 --rm --name medical-portal-ui medical-portal-ui`

## Overview

This directory contains end-to-end tests (against the GettingStarted project) using [Postman](https://www.postman.com/).

First, you need to run the GettingStarted project (in Kestrel or IIS Express).

## Running the tests manually

The collection can be imported in Postman to run the tests one-by-one.
How to use tests in Postman is described [here](https://blog.postman.com/writing-tests-in-postman/) and
[here](https://learning.postman.com/docs/writing-scripts/script-references/test-examples/), but in short:
- The 'Tests' tab (top panel) contains assertions in JavaScript.
- After send, the outcome is displayed on the 'Test Results' tab (bottom panel).

## Running all tests sequentially

Newman is used to run all tests from the command-line. Detailed steps are described at [this link](https://learning.postman.com/docs/running-collections/using-newman-cli/command-line-integration-with-newman/).
Quick steps:
- Install [Node.js](https://nodejs.org/en/download/) including NPM
- Install Newman:
	```
	npm install -g newman
	```
- Run the tests:
```
	newman run JADNC_GettingStarted_PostmanCollection.json --globals JADNC_GettingStarted_PostmanGlobals.json
```

## Running all tests in parallel

This is implemented using a custom script, based on [this](https://medium.com/@mnu/run-multiple-postman-collection-in-parallel-stress-ee20801922ed). All you need to run:
```
npm install
npm start
```

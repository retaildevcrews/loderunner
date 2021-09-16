# Setup VSCode for PlantUML Rendering

## Install VSCode Extension

Launch VS Code Quick Open (Ctrl+P), paste the following command, and press enter.

`ext install plantuml`

Alternatively, from the command line you may install with the following command:

```bash
code --install-extension jebbs.plantuml@2.15.1
```

## Update Settings

1. Open VSCode settings using `Ctrl+,`
2. Enter `@ext:jebbs.plantuml` into the search bar
3. Click the `Edit in settings.json` link
4. Add/replace the following settings to the bottom of settings json:

```json

    "plantuml.diagramsRoot": "/docs/diagrams/source",
    "plantuml.exportFormat": "svg",
    "plantuml.exportOutDir": "/docs/diagrams/out",
    "plantuml.exportSubFolder": false,
    "plantuml.includepaths": [],
    "plantuml.commandArgs": [],
    "plantuml.render": "PlantUMLServer",
    "plantuml.server": "https://www.plantuml.com/plantuml",
    "plantuml.urlFormat": "svg",
    "plantuml.urlResult": "SimpleURL"
```

The settings above use the remote plantuml server to render the image.  If you want to setup local rendering please refer to the guidance in the PlantUML extension for configuration and setup.

## Creating a Diagram

1. Create a new file in `/docs/diagrams/source` using a `.puml` extension.
2. Add diagram UML to new file using [PlantUML syntax](http://plantuml.com/guide)
3. Right-click on editing window and select "Preview Current Diagram" to see a split window rendering.
4. When complete right click and select "Export File Diagrams" to render to the output folder.
5. Alternatively, you may right click on the `source` folder and click `Export Workspace Diagrams` to generate for all of the `.puml` files in `/source`

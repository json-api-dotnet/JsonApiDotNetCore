rm -rf _site
docfx ./docfx.json
cp home/index.html _site/index.html
cp home/favicon.ico _site/favicon.ico
cp -R home/assets/* _site/styles/

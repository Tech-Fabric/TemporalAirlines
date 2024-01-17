# Development

This project requires `sass` executable to be available in PATH, just download it from github: https://github.com/sass/dart-sass/releases
and add it to PATH environment variable.

Use following script for linux:

```bash
sudo apt update
sudo apt install curl
mkdir -p tmp
cd tmp
curl -L -o sass_package.tar.gz https://github.com/sass/dart-sass/releases/download/1.55.0/dart-sass-1.55.0-linux-x64.tar.gz
tar -xvf ./sass_package.tar.gz
mkdir -p ~/.local/bin/
cp dart-sass/sass ~/.local/bin/
cd -
rm -Rf tmp
```

Please don't use npm to install sass, that version is slower.

More details here: https://sass-lang.com/install/

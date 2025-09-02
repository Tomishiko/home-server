#! /bin/bash
npx esbuild Scripts/main.ts --bundle --splitting --outdir=wwwroot/js --format=esm  --target=es2017
# --chunk-names='bundle-[hash]'

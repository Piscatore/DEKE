#!/bin/bash
# Download the all-MiniLM-L6-v2 ONNX model for embeddings

set -e

MODEL_DIR="models/all-MiniLM-L6-v2"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

cd "$PROJECT_ROOT"

echo "Creating model directory..."
mkdir -p "$MODEL_DIR"

echo "Downloading all-MiniLM-L6-v2 ONNX model..."

# Download model.onnx
if [ ! -f "$MODEL_DIR/model.onnx" ]; then
    echo "  → Downloading model.onnx..."
    curl -L -o "$MODEL_DIR/model.onnx" \
        "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx"
else
    echo "  → model.onnx already exists, skipping..."
fi

# Download vocab.txt
if [ ! -f "$MODEL_DIR/vocab.txt" ]; then
    echo "  → Downloading vocab.txt..."
    curl -L -o "$MODEL_DIR/vocab.txt" \
        "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt"
else
    echo "  → vocab.txt already exists, skipping..."
fi

# Verify downloads
echo ""
echo "Verifying downloads..."
if [ -f "$MODEL_DIR/model.onnx" ] && [ -f "$MODEL_DIR/vocab.txt" ]; then
    MODEL_SIZE=$(du -h "$MODEL_DIR/model.onnx" | cut -f1)
    VOCAB_LINES=$(wc -l < "$MODEL_DIR/vocab.txt")
    echo "  ✓ model.onnx: $MODEL_SIZE"
    echo "  ✓ vocab.txt: $VOCAB_LINES tokens"
    echo ""
    echo "Model download complete!"
else
    echo "  ✗ Download failed - please check your internet connection"
    exit 1
fi

FROM amd64/mono:5.16.0
WORKDIR /workspace

RUN apt-get update && apt-get install -y zip && rm -rf /var/lib/apt/lists/*
COPY CultistSimulator/*.dll /cs-dlls/

CMD tail -f /dev/null

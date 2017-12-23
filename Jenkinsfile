pipeline {
  agent {
    docker { image 'microsoft/dotnet:sdk' }
  }
  stages {
    stage('Build') {
      steps {
        sh 'dotnet restore'
        sh 'dotnet test src/ModSink.Common.Tests/ModSink.Common.Tests.csproj'
      }
    }
  }
}

pipeline {
  agent {
    docker {
      image 'microsoft/dotnet:sdk'
    }
    
  }
  stages {
    stage('Build') {
      steps {
        sh 'dotnet restore'
      }
    }
    stage('Test') {
      parallel {
        stage('Test') {
          steps {
            sh 'dotnet test src/ModSink.Common.Tests/ModSink.Common.Tests.csproj'
          }
        }
        stage('Publish') {
          steps {
            sh 'dotnet pack src/ModSink.Common/ModSink.Common.csproj'
            archiveArtifacts 'src/ModSink.Common/bin/'
          }
        }
      }
    }
  }
}
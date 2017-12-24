pipeline {
  agent {
    docker {
      image 'microsoft/dotnet:sdk'
      args '--user root'
    }
    
  }
  stages {
    stage('Restore') {
      steps {
        sh 'dotnet restore'
      }
    }
    stage('Process') {
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

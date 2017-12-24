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
            sh '''cd ./src/ModSink.Common.Tests/
dotnet xunit -xml result.xml'''
            junit 'src/ModSink.Common.Tests/result.xml'
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
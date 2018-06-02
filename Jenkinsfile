pipeline {
  agent {
    docker {
      image 'microsoft/dotnet:2.0.4-sdk-2.1.3'
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
            sh 'cd src/ModSink.Common.Tests; dotnet restore'
            sh 'cd src/ModSink.Common.Tests; set +e; dotnet xunit -xml result.xml; if [ $? -eq 1 ]; then exit 0; fi; set -e;'
            step([$class: 'XUnitPublisher', testTimeMargin: '3000', thresholdMode: 2, thresholds: [[$class: 'FailedThreshold', failureNewThreshold: '1', failureThreshold: '10', unstableNewThreshold: '0', unstableThreshold: '0'], [$class: 'SkippedThreshold', failureNewThreshold: '1', failureThreshold: '10', unstableNewThreshold: '0', unstableThreshold: '0']], tools: [[$class: 'XUnitDotNetTestType', deleteOutputFiles: true, failIfNotNew: true, pattern: 'src/ModSink.Common.Tests/result.xml', skipNoTestFiles: false, stopProcessingIfError: true]]])
          }
        }
        stage('Publish') {
          steps {
            sh 'dotnet pack -c Release --include-symbols /p:TargetFrameworks=netstandard2.0 src/ModSink.Common/ModSink.Common.csproj'
            archiveArtifacts 'src/**/*.nupkg'
          }
        }
      }
    }
  }
}

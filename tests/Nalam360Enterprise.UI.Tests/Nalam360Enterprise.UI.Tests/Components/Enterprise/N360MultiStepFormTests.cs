using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Nalam360Enterprise.UI.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Models;
using Xunit;

namespace Nalam360Enterprise.UI.Tests.Components.Enterprise
{
    public class N360MultiStepFormTests : TestContext
    {
        public N360MultiStepFormTests()
        {
            Services.AddSingleton<IPermissionService, MockPermissionService>();
            Services.AddSingleton<IAuditService, MockAuditService>();
        }

        [Fact]
        public void MultiStepForm_RendersWithoutCrashing()
        {
            // Arrange
            var config = new MultiStepFormConfig
            {
                Steps = new List<FormStep>
                {
                    new() { Id = "step1", Title = "Personal Info", Order = 1 },
                    new() { Id = "step2", Title = "Contact", Order = 2 },
                    new() { Id = "step3", Title = "Review", Order = 3 }
                }
            };

            // Act
            var cut = RenderComponent<N360MultiStepForm>(parameters => parameters
                .Add(p => p.Configuration, config));

            // Assert
            Assert.NotNull(cut);
            Assert.Contains("n360-multistep-form", cut.Markup);
        }

        [Fact]
        public void MultiStepForm_DisplaysAllSteps()
        {
            // Arrange
            var config = new MultiStepFormConfig
            {
                Steps = new List<FormStep>
                {
                    new() { Id = "step1", Title = "Step 1", Order = 1 },
                    new() { Id = "step2", Title = "Step 2", Order = 2 },
                    new() { Id = "step3", Title = "Step 3", Order = 3 }
                }
            };

            // Act
            var cut = RenderComponent<N360MultiStepForm>(parameters => parameters
                .Add(p => p.Configuration, config));

            // Assert
            var stepIndicators = cut.FindAll(".n360-multistep-form__step");
            Assert.Equal(3, stepIndicators.Count);
        }

        [Fact]
        public void MultiStepForm_ShowsProgressBar()
        {
            // Arrange
            var config = new MultiStepFormConfig
            {
                ShowProgressBar = true,
                Steps = new List<FormStep>
                {
                    new() { Id = "step1", Title = "Step 1", Order = 1, IsCompleted = true },
                    new() { Id = "step2", Title = "Step 2", Order = 2 }
                }
            };

            // Act
            var cut = RenderComponent<N360MultiStepForm>(parameters => parameters
                .Add(p => p.Configuration, config));

            // Assert
            var progressBar = cut.Find(".n360-multistep-form__progress-bar");
            Assert.NotNull(progressBar);
            Assert.Contains("50%", progressBar.GetAttribute("style")); // 1 of 2 steps completed
        }

        [Fact]
        public void MultiStepForm_NavigatesNextStep()
        {
            // Arrange
            var config = new MultiStepFormConfig
            {
                Steps = new List<FormStep>
                {
                    new() { Id = "step1", Title = "Step 1", Order = 1 },
                    new() { Id = "step2", Title = "Step 2", Order = 2 }
                }
            };

            // Act
            var cut = RenderComponent<N360MultiStepForm>(parameters => parameters
                .Add(p => p.Configuration, config));

            var nextButton = cut.Find("button:contains('Next')");
            nextButton.Click();

            // Assert
            var activeStep = cut.Find(".n360-multistep-form__step--active");
            Assert.Contains("Step 2", activeStep.TextContent);
        }

        [Fact]
        public void MultiStepForm_NavigatesPreviousStep()
        {
            // Arrange
            var config = new MultiStepFormConfig
            {
                Steps = new List<FormStep>
                {
                    new() { Id = "step1", Title = "Step 1", Order = 1 },
                    new() { Id = "step2", Title = "Step 2", Order = 2 }
                }
            };

            // Act
            var cut = RenderComponent<N360MultiStepForm>(parameters => parameters
                .Add(p => p.Configuration, config)
                .Add(p => p.CurrentStepIndex, 1)); // Start at step 2

            var previousButton = cut.Find("button:contains('Previous')");
            previousButton.Click();

            // Assert
            var activeStep = cut.Find(".n360-multistep-form__step--active");
            Assert.Contains("Step 1", activeStep.TextContent);
        }

        [Fact]
        public void MultiStepForm_AllowsSkippingOptionalSteps()
        {
            // Arrange
            var config = new MultiStepFormConfig
            {
                AllowStepSkipping = true,
                Steps = new List<FormStep>
                {
                    new() { Id = "step1", Title = "Step 1", Order = 1 },
                    new() { Id = "step2", Title = "Optional Step", Order = 2, IsOptional = true },
                    new() { Id = "step3", Title = "Step 3", Order = 3 }
                }
            };

            // Act
            var cut = RenderComponent<N360MultiStepForm>(parameters => parameters
                .Add(p => p.Configuration, config)
                .Add(p => p.CurrentStepIndex, 1)); // At optional step

            var skipButton = cut.Find("button:contains('Skip')");
            skipButton.Click();

            // Assert
            var activeStep = cut.Find(".n360-multistep-form__step--active");
            Assert.Contains("Step 3", activeStep.TextContent);
        }

        [Fact]
        public void MultiStepForm_ValidatesSteps()
        {
            // Arrange
            var config = new MultiStepFormConfig
            {
                ValidateOnStepChange = true,
                Steps = new List<FormStep>
                {
                    new()
                    {
                        Id = "step1",
                        Title = "Step 1",
                        Order = 1,
                        ValidationErrors = new List<string> { "Name is required" }
                    }
                }
            };

            // Act
            var cut = RenderComponent<N360MultiStepForm>(parameters => parameters
                .Add(p => p.Configuration, config));

            // Assert
            Assert.Contains("Name is required", cut.Markup);
        }

        [Fact]
        public void MultiStepForm_SubmitsOnLastStep()
        {
            // Arrange
            var config = new MultiStepFormConfig
            {
                Steps = new List<FormStep>
                {
                    new() { Id = "step1", Title = "Step 1", Order = 1 },
                    new() { Id = "step2", Title = "Step 2", Order = 2 }
                }
            };

            var submitted = false;
            Action<FormSubmissionData> submitCallback = _ => submitted = true;

            // Act
            var cut = RenderComponent<N360MultiStepForm>(parameters => parameters
                .Add(p => p.Configuration, config)
                .Add(p => p.CurrentStepIndex, 1) // At last step
                .Add(p => p.OnSubmit, submitCallback));

            var submitButton = cut.Find("button:contains('Submit')");
            submitButton.Click();

            // Assert
            Assert.True(submitted);
        }
    }
}

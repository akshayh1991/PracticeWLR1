using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using SecMan.Interfaces.BL;
using SecMan.Model;
using SecMan.UnitTests.Logger;
using Serilog;
using System.Net;
using UserAccessManagement.Controllers;
using UserAccessManagement.Handler;

namespace SecMan.UnitTests.Controller
{
    [CustomLogging]
    [Collection("Sequential Collection")]
    public class RoleControllerTests
    {
        private readonly Mock<IRoleBL> _mockRoleBL;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly RoleController _roleController;

        public RoleControllerTests()
        {
            _mockRoleBL = new Mock<IRoleBL>();
            _mockMediator = new Mock<IMediator>();
            _mockConfiguration = new Mock<IConfiguration>();
            _roleController = new RoleController(_mockRoleBL.Object, _mockMediator.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetAllRoles_ReturnsOkResult_WithListOfRoles()
        {
            // Arrange
            List<GetRoleDto> roleList = new List<GetRoleDto>
            {
                new GetRoleDto { Id = 1, Name = "Admin", Description = "Admin role", IsLoggedOutType = false, NoOfUsers = 10 },
                new GetRoleDto { Id = 2, Name = "User", Description = "User role", IsLoggedOutType = true, NoOfUsers = 5 }
            };

            _mockRoleBL.Setup(service => service.GetAllRolesAsync()).ReturnsAsync(roleList);

            // Act
            Log.Information("Calling GetAllRoles in RoleController");
            IActionResult result = await _roleController.GetAllRoles();

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<GetRoleDto> returnRoles = Assert.IsAssignableFrom<IEnumerable<GetRoleDto>>(okResult.Value);
            Assert.Equal(roleList.Count, ((List<GetRoleDto>)returnRoles).Count);

            Log.Information("Asserted that the response count matches the expected role count");

            // Verify that logging via IMediator occurred twice
            _mockMediator.Verify(m => m.Send(It.IsAny<InfoLogCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified that logging via IMediator occurred exactly twice");
        }

        [Fact]
        public async Task GetAllRoles_ReturnsOkResult_WhenNoRolesExist()
        {
            // Arrange
            _mockRoleBL.Setup(service => service.GetAllRolesAsync()).ReturnsAsync(new List<GetRoleDto>());

            // Act
            Log.Information("Calling GetAllRoles in RoleController");
            IActionResult result = await _roleController.GetAllRoles();

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<GetRoleDto> returnRoles = Assert.IsAssignableFrom<IEnumerable<GetRoleDto>>(okResult.Value);
            Assert.Empty(returnRoles);

            Log.Information("Asserted that the response contains an empty role list as expected");

            // Verify that logging via IMediator occurred twice
            _mockMediator.Verify(m => m.Send(It.IsAny<InfoLogCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified that logging via IMediator occurred exactly twice");
        }

        [Fact]
        public async Task GetRoleById_ReturnsOkResult_WithRoleDetails()
        {
            // Arrange
            ulong roleId = 1;
            GetRoleDto role = new GetRoleDto
            {
                Id = roleId,
                Name = "Admin",
                Description = "Admin role",
                IsLoggedOutType = false,
                NoOfUsers = 10
            };

            _mockRoleBL.Setup(service => service.GetRoleByIdAsync(roleId))
                .ReturnsAsync(new ServiceResponse<GetRoleDto> { StatusCode = HttpStatusCode.OK, Data = role });

            // Act
            Log.Information("Calling GetRoleById in RoleController");
            IActionResult result = await _roleController.GetRoleById(roleId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            GetRoleDto returnRole = Assert.IsAssignableFrom<GetRoleDto>(okResult.Value);
            Assert.Equal(roleId, returnRole.Id);
            Assert.Equal("Admin", returnRole.Name);

            Log.Information("Asserted that the returned role matches the expected role");

            // Verify that logging via IMediator occurred twice
            _mockMediator.Verify(m => m.Send(It.IsAny<InfoLogCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified that logging via IMediator occurred exactly twice");
        }

        [Fact]
        public async Task AddRole_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            CreateRole dto = new CreateRole();  // Assuming an invalid dto
            _roleController.ModelState.AddModelError("Name", "The Name field is required.");
            Log.Information("ModelState added with validation error for 'Name'.");

            // Act
            Log.Information("Calling AddRole in RoleController");
            IActionResult result = await _roleController.AddRole(dto);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            SerializableError errors = Assert.IsType<SerializableError>(badRequestResult.Value);
            Log.Information("Asserted BadRequest response with model errors");

            // Assert additional details (model errors)
            Assert.NotEmpty(errors);  // Ensure that there are model state errors
            Log.Information("ModelState contains errors as expected");

            // Complete the test with a log
            Log.Information("Test AddRole_ReturnsBadRequest_WhenModelIsInvalid completed");
        }

        [Fact]
        public async Task AddRole_ReturnsBadRequest_WhenRoleNameIsInvalid()
        {
            // Arrange
            CreateRole dto = new CreateRole { Name = "InvalidRoleName" };

            ServiceResponse<string> mockResponse = new ServiceResponse<string>
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Role name is invalid",
                Data = null
            };

            // Mock the ValidateRoleNameAsync method to return the above response
            _mockRoleBL.Setup(x => x.ValidateRoleNameAsync(dto.Name))
                       .ReturnsAsync(mockResponse);

            Log.Information("Mocked role name validation to return BadRequest.");

            // Act
            Log.Information("Calling AddRole in RoleController with invalid role name");
            IActionResult result = await _roleController.AddRole(dto);

            // Assert
            BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            BadRequest response = Assert.IsType<BadRequest>(badRequestResult.Value);
            Log.Information("Asserted BadRequest response with message: {Message}", response.Detail);

            // Verify the response contains the expected message
            Assert.Equal("Role name is invalid", response.Detail);
            Log.Information("Test AddRole_ReturnsBadRequest_WhenRoleNameIsInvalid completed");
        }

        [Fact]
        public async Task UpdateRole_ReturnsOkResult_WithUpdatedRoleDetails()
        {
            // Arrange
            ulong roleId = 1;
            UpdateRole updateRoleDto = new UpdateRole { Name = "Updated Role", Description = "Updated Description" };

            ServiceResponse<GetRoleDto?> existingRole = new ServiceResponse<GetRoleDto?>
            {
                StatusCode = HttpStatusCode.OK,
                Data = new GetRoleDto
                {
                    Id = roleId,
                    Name = "Old Role",
                    Description = "Old Description",
                    IsLoggedOutType = false,
                    NoOfUsers = 5
                }
            };

            ServiceResponse<GetRoleDto?> updatedRole = new ServiceResponse<GetRoleDto?>
            {
                StatusCode = HttpStatusCode.OK,
                Data = new GetRoleDto
                {
                    Id = roleId,
                    Name = updateRoleDto.Name,
                    Description = updateRoleDto.Description,
                    IsLoggedOutType = false,
                    NoOfUsers = 10
                }
            };

            _mockRoleBL.Setup(bl => bl.GetRoleByIdAsync(roleId)).ReturnsAsync(existingRole);
            _mockRoleBL.Setup(bl => bl.ExistingRoleNameWhileUpdation(updateRoleDto.Name, roleId))
                       .ReturnsAsync(new ServiceResponse<string>
                       {
                           StatusCode = HttpStatusCode.OK,
                           Data = string.Empty
                       });
            _mockRoleBL.Setup(bl => bl.UpdateRoleAsync(roleId, updateRoleDto, false)).ReturnsAsync(updatedRole);

            // Act
            IActionResult result = await _roleController.UpdateRole(roleId, updateRoleDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            UpdatedResponse responseContent = Assert.IsType<UpdatedResponse>(okResult.Value);

            Assert.Equal(roleId, responseContent.Id);

            // Verify logging via IMediator
            _mockMediator.Verify(m => m.Send(It.IsAny<InfoLogCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified positive case for UpdateRole");
        }

        [Fact]
        public async Task UpdateRole_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            ulong roleId = 999;
            UpdateRole updateRoleDto = new UpdateRole { Name = "Nonexistent Role", Description = "Does not exist" };

            _mockRoleBL.Setup(bl => bl.GetRoleByIdAsync(roleId))
                       .ReturnsAsync(new ServiceResponse<GetRoleDto?>
                       {
                           StatusCode = HttpStatusCode.NotFound,
                           Message = "Role not found"
                       });

            // Act
            IActionResult result = await _roleController.UpdateRole(roleId, updateRoleDto);

            // Assert
            NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            NotFound responseContent = Assert.IsType<NotFound>(notFoundResult.Value);

            Assert.Equal("Role not found", responseContent.Detail);

            Log.Information("Verified NotFound case for UpdateRole");
        }

        [Fact]
        public async Task UpdateRole_ReturnsConflict_WhenRoleNameAlreadyExists()
        {
            // Arrange
            ulong roleId = 1;
            UpdateRole updateRoleDto = new UpdateRole { Name = "Duplicate Role", Description = "Duplicate Name Conflict" };

            ServiceResponse<GetRoleDto?> existingRole = new ServiceResponse<GetRoleDto?>
            {
                StatusCode = HttpStatusCode.OK,
                Data = new GetRoleDto
                {
                    Id = roleId,
                    Name = "Old Role",
                    Description = "Old Description",
                    IsLoggedOutType = false,
                    NoOfUsers = 5
                }
            };

            _mockRoleBL.Setup(bl => bl.GetRoleByIdAsync(roleId)).ReturnsAsync(existingRole);
            _mockRoleBL.Setup(bl => bl.ExistingRoleNameWhileUpdation(updateRoleDto.Name, roleId))
                       .ReturnsAsync(new ServiceResponse<string>
                       {
                           StatusCode = HttpStatusCode.Conflict,
                           Message = "Role name already exists"
                       });

            // Act
            IActionResult result = await _roleController.UpdateRole(roleId, updateRoleDto);

            // Assert
            ConflictObjectResult conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Conflict responseContent = Assert.IsType<Conflict>(conflictResult.Value);

            Assert.Equal("Role name already exists", responseContent.Detail);

            Log.Information("Verified Conflict case for UpdateRole");
        }

        [Fact]
        public async Task UpdateRole_ReturnsConflict_WhenUpdateFails()
        {
            // Arrange
            ulong roleId = 1;
            UpdateRole updateRoleDto = new UpdateRole { Name = "Valid Role", Description = "Update Failure Case" };

            ServiceResponse<GetRoleDto?> existingRole = new ServiceResponse<GetRoleDto?>
            {
                StatusCode = HttpStatusCode.OK,
                Data = new GetRoleDto
                {
                    Id = roleId,
                    Name = "Old Role",
                    Description = "Old Description",
                    IsLoggedOutType = false,
                    NoOfUsers = 5
                }
            };

            _mockRoleBL.Setup(bl => bl.GetRoleByIdAsync(roleId)).ReturnsAsync(existingRole);
            _mockRoleBL.Setup(bl => bl.ExistingRoleNameWhileUpdation(updateRoleDto.Name, roleId))
                       .ReturnsAsync(new ServiceResponse<string>
                       {
                           StatusCode = HttpStatusCode.OK,
                           Data = string.Empty
                       });
            _mockRoleBL.Setup(bl => bl.UpdateRoleAsync(roleId, updateRoleDto, false))
                       .ReturnsAsync(new ServiceResponse<GetRoleDto?>
                       {
                           StatusCode = HttpStatusCode.Conflict,
                           Message = "Failed to update role"
                       });

            // Act
            IActionResult result = await _roleController.UpdateRole(roleId, updateRoleDto);

            // Assert
            ConflictObjectResult conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Conflict responseContent = Assert.IsType<Conflict>(conflictResult.Value);

            Assert.Equal("Failed to update role", responseContent.Detail);

            Log.Information("Verified Conflict case for UpdateRole when update fails");
        }

        [Fact]
        public async Task DeleteRole_ReturnsNoContent_WhenRoleDeletedSuccessfully()
        {
            // Arrange
            ulong roleId = 1;

            ServiceResponse<GetRoleDto?> existingRole = new ServiceResponse<GetRoleDto?>
            {
                StatusCode = HttpStatusCode.OK,
                Data = new GetRoleDto
                {
                    Id = roleId,
                    Name = "Test Role",
                    Description = "Test Description",
                    IsLoggedOutType = false,
                    NoOfUsers = 10
                }
            };

            _mockRoleBL.Setup(bl => bl.GetRoleByIdAsync(roleId)).ReturnsAsync(existingRole);
            _mockRoleBL.Setup(bl => bl.DeleteRoleAsync(roleId, false))
                       .Returns(Task.FromResult(new ApiResponse
                       {
                           StatusCode = HttpStatusCode.NoContent
                       }));

            // Act
            IActionResult result = await _roleController.DeleteRole(roleId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify logging via IMediator
            _mockMediator.Verify(m => m.Send(It.IsAny<InfoLogCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            Log.Information("Verified positive case for DeleteRole");
        }

        [Fact]
        public async Task DeleteRole_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            // Arrange
            ulong roleId = 999;

            _mockRoleBL.Setup(bl => bl.GetRoleByIdAsync(roleId))
                       .ReturnsAsync(new ServiceResponse<GetRoleDto?>
                       {
                           StatusCode = HttpStatusCode.NotFound,
                           Message = "Role not found"
                       });

            // Act
            IActionResult result = await _roleController.DeleteRole(roleId);

            // Assert
            NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            NotFound responseContent = Assert.IsType<NotFound>(notFoundResult.Value);

            Assert.Equal("Role not found", responseContent.Detail);

            // Verify only the GetRoleByIdAsync method was called
            _mockRoleBL.Verify(bl => bl.GetRoleByIdAsync(roleId), Times.Once);
            _mockRoleBL.Verify(bl => bl.DeleteRoleAsync(It.IsAny<ulong>(), false), Times.Never);

            Log.Information("Verified NotFound case for DeleteRole");
        }
    }
}

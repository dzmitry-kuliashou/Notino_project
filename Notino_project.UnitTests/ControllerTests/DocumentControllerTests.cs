using Microsoft.AspNetCore.Mvc;
using MediatR;
using Moq;
using Notino_project.Controllers;
using Notino_project.Application.Commands;
using Notino_project.Application.Queries;
using Notino_project.Contract.Requests;
using Notino_project.Contract.Responses;
using Xunit;

namespace Notino_project.UnitTests.ControllerTests
{
    public class DocumentControllerTests
    {
        [Fact]
        public async Task List_DispatchesQueryAndReturnsHandlerResult()
        {
            var parameters = new ListParams { Page = "2", PageSize = "7" };
            var handlerResult = new OkObjectResult(new DocumentsResponse());
            var sender = new Mock<ISender>();
            sender.Setup(mock => mock.Send(It.IsAny<ListDocumentsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(handlerResult);
            var controller = new DocumentController(sender.Object);

            var result = await controller.List(parameters);

            Assert.Same(handlerResult, result);
            sender.Verify(
                mock => mock.Send(
                    It.Is<ListDocumentsQuery>(query =>
                        query.Parameters.Page == parameters.Page &&
                        query.Parameters.PageSize == parameters.PageSize),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_DispatchesQueryAndReturnsHandlerResult()
        {
            var id = Guid.NewGuid();
            var handlerResult = new NotFoundResult();
            var sender = new Mock<ISender>();
            sender.Setup(mock => mock.Send(It.IsAny<GetByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(handlerResult);
            var controller = new DocumentController(sender.Object);

            var result = await controller.GetById(id);

            Assert.Same(handlerResult, result);
            sender.Verify(
                mock => mock.Send(
                    It.Is<GetByIdQuery>(query => query.Id == id),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_DocumentExists_ReturnsDocumentResponse()
        {
            var id = Guid.NewGuid();

            var sender = new Mock<ISender>();
            sender.Setup(mock => mock.Send(It.IsAny<GetByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OkObjectResult(new DocumentResponse { Id = id }));
            var controller = new DocumentController(sender.Object);

            var result = await controller.GetById(id);

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<DocumentResponse>(((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task Create_DispatchesCommandAndReturnsHandlerResult()
        {
            var request = new CreateDocumentRequest();
            var handlerResult = new AcceptedResult();
            var sender = new Mock<ISender>();
            sender.Setup(mock => mock.Send(It.IsAny<CreateDocumentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(handlerResult);
            var controller = new DocumentController(sender.Object);

            var result = await controller.Create(request);

            Assert.Same(handlerResult, result);
            sender.Verify(
                mock => mock.Send(
                    It.Is<CreateDocumentCommand>(command => ReferenceEquals(command.Request, request)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_DispatchesCommandAndReturnsHandlerResult()
        {
            var id = Guid.NewGuid();
            var request = new UpdateDocumentRequest();
            var handlerResult = new AcceptedResult();
            var sender = new Mock<ISender>();
            sender.Setup(mock => mock.Send(It.IsAny<UpdateDocumentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(handlerResult);
            var controller = new DocumentController(sender.Object);

            var result = await controller.Update(id, request);

            Assert.Same(handlerResult, result);
            sender.Verify(
                mock => mock.Send(
                    It.Is<UpdateDocumentCommand>(command =>
                        command.Id == id && ReferenceEquals(command.Request, request)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
